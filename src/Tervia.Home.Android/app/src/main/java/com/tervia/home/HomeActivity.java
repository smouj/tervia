package com.tervia.home;

import android.app.Activity;
import android.app.role.RoleManager;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.content.pm.ResolveInfo;
import android.graphics.Color;
import android.graphics.Typeface;
import android.graphics.drawable.GradientDrawable;
import android.graphics.drawable.Drawable;
import android.os.Build;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.provider.Settings;
import android.view.Gravity;
import android.view.View;
import android.view.Window;
import android.view.WindowManager;
import android.widget.Button;
import android.widget.FrameLayout;
import android.widget.GridLayout;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ScrollView;
import android.widget.TextView;
import android.widget.Toast;

import org.json.JSONException;

import java.text.DateFormat;
import java.util.ArrayList;
import java.util.Comparator;
import java.util.Date;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Set;

/** TV-first home screen. The OEM launcher remains installed and selectable in Android settings. */
public final class HomeActivity extends Activity {
    private static final int REQUEST_HOME_ROLE = 71;
    private static final String EXTRA_PROFILE = "profileBase64";
    private static final String EXTRA_ACTION = "action";
    private static final String ACTION_APPLY_PROFILE = "com.tervia.home.APPLY_PROFILE";

    private final List<TvApp> applications = new ArrayList<>();
    private final Handler clockHandler = new Handler(Looper.getMainLooper());
    private final Runnable clockUpdater = new Runnable() {
        @Override
        public void run() {
            if (clock == null || isFinishing()) {
                return;
            }
            clock.setText(DateFormat.getTimeInstance(DateFormat.SHORT, Locale.getDefault()).format(new Date()));
            clockHandler.postDelayed(this, 30_000);
        }
    };
    private LauncherProfile profile = LauncherProfile.defaults();
    private LinearLayout content;
    private TextView clock;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        enterImmersiveMode();
        loadSavedProfile();
        acceptProfile(getIntent());
        render();
        handleLaunchAction(getIntent());
    }

    @Override
    protected void onNewIntent(Intent intent) {
        super.onNewIntent(intent);
        setIntent(intent);
        if (acceptProfile(intent)) {
            render();
        }
        handleLaunchAction(intent);
    }

    @Override
    protected void onResume() {
        super.onResume();
        refreshApplications();
        if (content != null) {
            render();
        }
    }

    @Override
    protected void onPause() {
        clockHandler.removeCallbacks(clockUpdater);
        super.onPause();
    }

    @Override
    protected void onDestroy() {
        clockHandler.removeCallbacks(clockUpdater);
        clock = null;
        super.onDestroy();
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if (requestCode == REQUEST_HOME_ROLE) {
            render();
            String message = resultCode == RESULT_OK
                    ? "Tervia Home ya es la pantalla de inicio."
                    : "Puedes elegir Tervia Home más tarde desde Ajustes.";
            Toast.makeText(this, message, Toast.LENGTH_LONG).show();
        }
    }

    private boolean acceptProfile(Intent intent) {
        if (intent == null || !(ACTION_APPLY_PROFILE.equals(intent.getAction())
                || intent.hasExtra(EXTRA_PROFILE))) {
            return false;
        }

        String encoded = intent.getStringExtra(EXTRA_PROFILE);
        if (encoded == null) {
            return false;
        }

        try {
            LauncherProfile next = LauncherProfile.decode(encoded);
            profile = next;
            getSharedPreferences("tervia-home", MODE_PRIVATE).edit()
                    .putString(LauncherProfile.PREFERENCE_KEY, next.toJson().toString())
                    .apply();
            return true;
        } catch (JSONException exception) {
            Toast.makeText(this, "No se aplicó la personalización: " + exception.getMessage(), Toast.LENGTH_LONG).show();
            return false;
        }
    }

    private void loadSavedProfile() {
        String saved = getSharedPreferences("tervia-home", MODE_PRIVATE)
                .getString(LauncherProfile.PREFERENCE_KEY, null);
        if (saved == null) {
            return;
        }
        try {
            profile = LauncherProfile.decode(android.util.Base64.encodeToString(
                    saved.getBytes(java.nio.charset.StandardCharsets.UTF_8), android.util.Base64.NO_WRAP));
        } catch (JSONException exception) {
            getSharedPreferences("tervia-home", MODE_PRIVATE).edit()
                    .remove(LauncherProfile.PREFERENCE_KEY).apply();
            profile = LauncherProfile.defaults();
        }
    }

    private void refreshApplications() {
        Map<String, TvApp> byPackage = new HashMap<>();
        queryApplications(Intent.CATEGORY_LEANBACK_LAUNCHER, byPackage);
        queryApplications(Intent.CATEGORY_LAUNCHER, byPackage);

        Set<String> hidden = new HashSet<>(profile.hiddenPackages);
        applications.clear();
        for (TvApp app : byPackage.values()) {
            if (!app.packageName.equals(getPackageName()) && !hidden.contains(app.packageName)) {
                applications.add(app);
            }
        }

        Map<String, Integer> pinnedOrder = new HashMap<>();
        for (int index = 0; index < profile.pinnedPackages.size(); index++) {
            pinnedOrder.put(profile.pinnedPackages.get(index), index);
        }
        applications.sort(Comparator
                .comparingInt((TvApp app) -> pinnedOrder.getOrDefault(app.packageName, Integer.MAX_VALUE))
                .thenComparing(app -> app.label.toLowerCase(Locale.ROOT)));
    }

    private void queryApplications(String category, Map<String, TvApp> destination) {
        Intent query = new Intent(Intent.ACTION_MAIN).addCategory(category);
        List<ResolveInfo> results;
        if (Build.VERSION.SDK_INT >= 33) {
            results = getPackageManager().queryIntentActivities(query,
                    PackageManager.ResolveInfoFlags.of(PackageManager.MATCH_ALL));
        } else {
            //noinspection deprecation
            results = getPackageManager().queryIntentActivities(query, PackageManager.MATCH_ALL);
        }

        for (ResolveInfo item : results) {
            if (item.activityInfo == null || !item.activityInfo.exported || !item.activityInfo.enabled) {
                continue;
            }
            String packageName = item.activityInfo.packageName;
            String label = item.loadLabel(getPackageManager()).toString().trim();
            if (label.isEmpty()) {
                label = packageName;
            }
            TvApp existing = destination.get(packageName);
            if (existing == null || category.equals(Intent.CATEGORY_LEANBACK_LAUNCHER)) {
                destination.put(packageName, new TvApp(
                        packageName,
                        item.activityInfo.name,
                        label,
                        item.loadIcon(getPackageManager())));
            }
        }
    }

    private void render() {
        int accent = LauncherProfile.accentColor(profile.theme);
        ScrollView scroll = new ScrollView(this);
        scroll.setFillViewport(true);
        scroll.setBackgroundColor(Color.rgb(7, 18, 32));

        content = new LinearLayout(this);
        content.setOrientation(LinearLayout.VERTICAL);
        content.setPadding(dp(52), dp(30), dp(52), dp(34));
        scroll.addView(content, new ScrollView.LayoutParams(
                ScrollView.LayoutParams.MATCH_PARENT, ScrollView.LayoutParams.WRAP_CONTENT));

        FrameLayout root = new FrameLayout(this);
        root.setBackground(new GradientDrawable(GradientDrawable.Orientation.TL_BR,
                new int[]{Color.rgb(7, 18, 32), Color.rgb(10, 35, 62), Color.rgb(11, 25, 45)}));
        root.addView(scroll, new FrameLayout.LayoutParams(
                FrameLayout.LayoutParams.MATCH_PARENT, FrameLayout.LayoutParams.MATCH_PARENT));
        setContentView(root);
        clockHandler.removeCallbacks(clockUpdater);

        content.addView(buildHeader(accent));
        content.addView(buildHero(accent), marginParams(-1, -2, 0, 28, 0, 25));

        TextView sectionTitle = text("Tus aplicaciones", 23, Color.WHITE, Typeface.BOLD);
        content.addView(sectionTitle, marginParams(-1, -2, 2, 0, 0, 5));
        TextView sectionHint = text("Selecciona una aplicación con el mando de tu televisor.", 15,
                0xFFAFBED0, Typeface.NORMAL);
        content.addView(sectionHint, marginParams(-1, -2, 2, 0, 0, 14));
        content.addView(buildAppGrid(accent), marginParams(-1, -2, -10, 0, -10, 24));

        TextView footer = text("La pantalla original de la TV se conserva. Puedes volver a elegirla desde Ajustes.",
                13, 0xFF9EADC0, Typeface.NORMAL);
        content.addView(footer, marginParams(-1, -2, 2, 10, 0, 0));
    }

    private View buildHeader(int accent) {
        LinearLayout row = new LinearLayout(this);
        row.setGravity(Gravity.CENTER_VERTICAL);

        ImageView logo = new ImageView(this);
        logo.setImageResource(com.tervia.home.R.drawable.tervia_logo);
        logo.setScaleType(ImageView.ScaleType.FIT_CENTER);
        row.addView(logo, new LinearLayout.LayoutParams(dp(42), dp(42)));

        LinearLayout brand = new LinearLayout(this);
        brand.setOrientation(LinearLayout.VERTICAL);
        brand.setPadding(dp(12), 0, 0, 0);
        TextView brandName = text("TERVIA HOME", 15, Color.WHITE, Typeface.BOLD);
        TextView brandCaption = text("TU TELEVISOR, A TU MANERA", 10, 0xFFACBED2, Typeface.NORMAL);
        brand.addView(brandName);
        brand.addView(brandCaption);
        row.addView(brand, new LinearLayout.LayoutParams(0, -2, 1));

        if (profile.showClock) {
            clock = text("", 20, Color.WHITE, Typeface.BOLD);
            clock.setGravity(Gravity.CENTER_VERTICAL | Gravity.RIGHT);
            updateClock();
            row.addView(clock, new LinearLayout.LayoutParams(-2, dp(42)));
        } else {
            clock = null;
        }
        return row;
    }

    private View buildHero(int accent) {
        LinearLayout panel = new LinearLayout(this);
        panel.setOrientation(LinearLayout.VERTICAL);
        panel.setPadding(dp(26), dp(22), dp(26), dp(22));
        GradientDrawable background = new GradientDrawable(GradientDrawable.Orientation.LEFT_RIGHT,
                new int[]{0xE6112F52, 0xCC102447});
        background.setCornerRadius(dp(22));
        background.setStroke(dp(1), 0x5539D8F2);
        panel.setBackground(background);

        TextView eyebrow = text("UN INICIO MÁS TUYO", 11, accent, Typeface.BOLD);
        TextView title = text(profile.title, 33, Color.WHITE, Typeface.BOLD);
        TextView subtitle = text(profile.subtitle, 17, 0xFFCCDAE9, Typeface.NORMAL);
        subtitle.setMaxLines(2);
        subtitle.setEllipsize(android.text.TextUtils.TruncateAt.END);
        panel.addView(eyebrow);
        panel.addView(title, marginParams(-1, -2, 0, 4, 0, 4));
        panel.addView(subtitle);

        Button setHome = new Button(this);
        setHome.setText(isHomeRoleHeld() ? "Tervia Home está seleccionada" : "Usar Tervia Home como inicio");
        setHome.setEnabled(!isHomeRoleHeld());
        setHome.setTextColor(Color.WHITE);
        setHome.setAllCaps(false);
        setHome.setTextSize(15);
        setHome.setOnClickListener(view -> requestHomeRole());
        setFocusableStyle(setHome, accent);
        panel.addView(setHome, marginParams(-2, 50, 0, 16, 0, 0));

        Button restore = new Button(this);
        restore.setText("Elegir la pantalla de inicio original");
        restore.setTextColor(0xFFB9C9DB);
        restore.setAllCaps(false);
        restore.setTextSize(13);
        restore.setOnClickListener(view -> openHomeSettings());
        setFocusableStyle(restore, accent);
        panel.addView(restore, marginParams(-2, 44, 0, 5, 0, 0));
        return panel;
    }

    private View buildAppGrid(int accent) {
        if (applications.isEmpty()) {
            TextView empty = text("No se encontraron aplicaciones de TV. Tervia Home sigue disponible; vuelve a abrir esta pantalla cuando instales apps compatibles.",
                    16, 0xFFBBC9D8, Typeface.NORMAL);
            empty.setPadding(dp(6), dp(14), dp(6), dp(14));
            return empty;
        }

        GridLayout grid = new GridLayout(this);
        int widthDp = (int) (getResources().getDisplayMetrics().widthPixels
                / getResources().getDisplayMetrics().density);
        grid.setColumnCount(Math.max(3, Math.min(6, widthDp / 190)));
        for (TvApp app : applications) {
            grid.addView(buildAppCard(app, accent));
        }
        return grid;
    }

    private View buildAppCard(TvApp app, int accent) {
        LinearLayout card = new LinearLayout(this);
        card.setOrientation(LinearLayout.VERTICAL);
        card.setGravity(Gravity.CENTER);
        card.setFocusable(true);
        card.setClickable(true);
        card.setPadding(dp(14), dp(10), dp(14), dp(11));
        card.setContentDescription(app.label);
        card.setBackground(cardBackground(false, accent));

        ImageView icon = new ImageView(this);
        icon.setImageDrawable(app.icon);
        icon.setScaleType(ImageView.ScaleType.FIT_CENTER);
        card.addView(icon, new LinearLayout.LayoutParams(dp(62), dp(62)));

        TextView label = text(app.label, 14, Color.WHITE, Typeface.BOLD);
        label.setMaxLines(2);
        label.setGravity(Gravity.CENTER);
        card.addView(label, marginParams(-1, -2, 0, dp(8), 0, 0));

        card.setOnFocusChangeListener((view, focused) -> {
            view.setBackground(cardBackground(focused, accent));
            view.setScaleX(focused ? 1.06f : 1f);
            view.setScaleY(focused ? 1.06f : 1f);
            view.setElevation(focused ? dp(8) : 0);
        });
        card.setOnClickListener(view -> launchApplication(app));

        GridLayout.LayoutParams params = new GridLayout.LayoutParams();
        params.width = dp(176);
        params.height = dp(146);
        params.setMargins(dp(8), dp(8), dp(8), dp(8));
        card.setLayoutParams(params);
        return card;
    }

    private GradientDrawable cardBackground(boolean focused, int accent) {
        GradientDrawable background = new GradientDrawable();
        background.setColor(focused ? 0xFF173B5D : 0xCC10243A);
        background.setCornerRadius(dp(16));
        background.setStroke(dp(focused ? 3 : 1), focused ? accent : 0x554B7599);
        return background;
    }

    private void setFocusableStyle(View view, int accent) {
        view.setBackground(cardBackground(false, accent));
        view.setOnFocusChangeListener((focusedView, focused) -> {
            focusedView.setScaleX(focused ? 1.04f : 1f);
            focusedView.setScaleY(focused ? 1.04f : 1f);
            focusedView.setElevation(focused ? dp(6) : 0);
            if (focusedView instanceof Button) {
                ((Button) focusedView).setBackground(cardBackground(focused, accent));
            }
        });
    }

    private void launchApplication(TvApp app) {
        try {
            Intent launch = new Intent(Intent.ACTION_MAIN)
                    .addCategory(Intent.CATEGORY_LAUNCHER)
                    .setClassName(app.packageName, app.activityName);
            launch.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
            startActivity(launch);
        } catch (Exception exception) {
            Toast.makeText(this, "No se pudo abrir " + app.label, Toast.LENGTH_SHORT).show();
            refreshApplications();
            render();
        }
    }

    private boolean isHomeRoleHeld() {
        if (Build.VERSION.SDK_INT < 29) {
            return false;
        }
        RoleManager manager = getSystemService(RoleManager.class);
        return manager != null && manager.isRoleAvailable(RoleManager.ROLE_HOME)
                && manager.isRoleHeld(RoleManager.ROLE_HOME);
    }

    private void requestHomeRole() {
        if (isHomeRoleHeld()) {
            return;
        }
        if (Build.VERSION.SDK_INT >= 29) {
            RoleManager manager = getSystemService(RoleManager.class);
            if (manager != null && manager.isRoleAvailable(RoleManager.ROLE_HOME)) {
                startActivityForResult(manager.createRequestRoleIntent(RoleManager.ROLE_HOME), REQUEST_HOME_ROLE);
                return;
            }
        }
        openHomeSettings();
    }

    private void handleLaunchAction(Intent intent) {
        if (intent == null) {
            return;
        }
        if (intent.getBooleanExtra("openHomeSettings", false)) {
            content.post(this::openHomeSettings);
        } else if (intent.getBooleanExtra("requestHomeRole", false) && !isHomeRoleHeld()) {
            content.post(this::requestHomeRole);
        }
    }

    private void openHomeSettings() {
        Intent settings = new Intent("android.settings.HOME_SETTINGS");
        if (settings.resolveActivity(getPackageManager()) == null) {
            settings = new Intent(Settings.ACTION_MANAGE_DEFAULT_APPS_SETTINGS);
        }
        try {
            startActivity(settings);
        } catch (Exception exception) {
            Toast.makeText(this, "Abre Ajustes > Aplicaciones > Aplicación de inicio para elegir otra pantalla.",
                    Toast.LENGTH_LONG).show();
        }
    }

    private void updateClock() {
        clockHandler.removeCallbacks(clockUpdater);
        if (clock != null) {
            clockUpdater.run();
        }
    }

    private void enterImmersiveMode() {
        Window window = getWindow();
        window.setFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN, WindowManager.LayoutParams.FLAG_FULLSCREEN);
        window.getDecorView().setSystemUiVisibility(
                View.SYSTEM_UI_FLAG_FULLSCREEN
                        | View.SYSTEM_UI_FLAG_HIDE_NAVIGATION
                        | View.SYSTEM_UI_FLAG_IMMERSIVE_STICKY
                        | View.SYSTEM_UI_FLAG_LAYOUT_STABLE
                        | View.SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION
                        | View.SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN);
    }

    private TextView text(String value, float size, int color, int style) {
        TextView view = new TextView(this);
        view.setText(value);
        view.setTextColor(color);
        view.setTextSize(size);
        view.setTypeface(Typeface.create("sans-serif", style));
        view.setGravity(Gravity.CENTER_VERTICAL);
        return view;
    }

    private LinearLayout.LayoutParams marginParams(int width, int height, int left, int top, int right, int bottom) {
        LinearLayout.LayoutParams params = new LinearLayout.LayoutParams(width < 0 ? width : dp(width), height < 0 ? height : dp(height));
        params.setMargins(dp(left), dp(top), dp(right), dp(bottom));
        return params;
    }

    private int dp(int value) {
        return Math.round(value * getResources().getDisplayMetrics().density);
    }

    private static final class TvApp {
        final String packageName;
        final String activityName;
        final String label;
        final Drawable icon;

        TvApp(String packageName, String activityName, String label, Drawable icon) {
            this.packageName = packageName;
            this.activityName = activityName;
            this.label = label;
            this.icon = icon;
        }
    }
}
