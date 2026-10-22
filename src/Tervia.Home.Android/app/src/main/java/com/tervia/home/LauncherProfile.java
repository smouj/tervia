package com.tervia.home;

import android.util.Base64;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashSet;
import java.util.List;
import java.util.Set;
import java.util.regex.Pattern;

/** Validated, bounded settings sent by the trusted local Tervia desktop app. */
final class LauncherProfile {
    static final String PREFERENCE_KEY = "profile.v1";
    static final String DEFAULT_TITLE = "Mi televisión";
    static final String DEFAULT_SUBTITLE = "Todo tu entretenimiento, en un mismo lugar.";
    static final int MAX_ENCODED_LENGTH = 12_000;

    private static final int MAX_JSON_BYTES = 8_192;
    private static final int MAX_APPS = 100;
    private static final Pattern PACKAGE_ID = Pattern.compile("[A-Za-z][A-Za-z0-9_]*(?:\\.[A-Za-z0-9_]+)+");
    private static final Set<String> THEMES;

    static {
        Set<String> themes = new HashSet<>();
        themes.add("ocean");
        themes.add("violet");
        themes.add("sunset");
        themes.add("forest");
        themes.add("rose");
        themes.add("gold");
        themes.add("ice");
        themes.add("graphite");
        THEMES = Collections.unmodifiableSet(themes);
    }

    final String title;
    final String subtitle;
    final String applicationsTitle;
    final String theme;
    final String cardLayout;
    final boolean showClock;
    final boolean showBranding;
    final List<String> pinnedPackages;
    final List<String> hiddenPackages;

    private LauncherProfile(
            String title,
            String subtitle,
            String applicationsTitle,
            String theme,
            String cardLayout,
            boolean showClock,
            boolean showBranding,
            List<String> pinnedPackages,
            List<String> hiddenPackages) {
        this.title = title;
        this.subtitle = subtitle;
        this.applicationsTitle = applicationsTitle;
        this.theme = theme;
        this.cardLayout = cardLayout;
        this.showClock = showClock;
        this.showBranding = showBranding;
        this.pinnedPackages = Collections.unmodifiableList(pinnedPackages);
        this.hiddenPackages = Collections.unmodifiableList(hiddenPackages);
    }

    static LauncherProfile defaults() {
        return new LauncherProfile(DEFAULT_TITLE, DEFAULT_SUBTITLE, "Tus aplicaciones", "ocean", "comfortable", true, true,
                Collections.emptyList(), Collections.emptyList());
    }

    static LauncherProfile decode(String encoded) throws JSONException {
        if (encoded == null || encoded.length() == 0 || encoded.length() > MAX_ENCODED_LENGTH) {
            throw new JSONException("El perfil enviado supera el tamaño permitido.");
        }

        final byte[] bytes;
        try {
            bytes = Base64.decode(encoded, Base64.DEFAULT);
        } catch (IllegalArgumentException exception) {
            throw new JSONException("El perfil enviado no tiene un formato válido.");
        }
        if (bytes.length == 0 || bytes.length > MAX_JSON_BYTES) {
            throw new JSONException("El perfil enviado supera el tamaño permitido.");
        }

        JSONObject json = new JSONObject(new String(bytes, StandardCharsets.UTF_8));
        if (json.optInt("version", 0) != 1) {
            throw new JSONException("La versión del perfil no es compatible.");
        }

        String title = cleanText(json.optString("title", DEFAULT_TITLE), DEFAULT_TITLE, 42);
        String subtitle = cleanText(json.optString("subtitle", DEFAULT_SUBTITLE), DEFAULT_SUBTITLE, 88);
        String theme = json.optString("theme", "ocean").toLowerCase(java.util.Locale.ROOT);
        if (!THEMES.contains(theme)) {
            theme = "ocean";
        }
        String cardLayout = json.optString("cardLayout", "comfortable").toLowerCase(java.util.Locale.ROOT);
        if (!"compact".equals(cardLayout) && !"comfortable".equals(cardLayout)) {
            cardLayout = "comfortable";
        }

        return new LauncherProfile(
                title,
                subtitle,
                cleanText(json.optString("applicationsTitle", "Tus aplicaciones"), "Tus aplicaciones", 36),
                theme,
                cardLayout,
                json.optBoolean("showClock", true),
                json.optBoolean("showBranding", true),
                readPackages(json.optJSONArray("pinnedApplicationIds")),
                readPackages(json.optJSONArray("hiddenApplicationIds")));
    }

    JSONObject toJson() throws JSONException {
        JSONObject json = new JSONObject();
        json.put("version", 1);
        json.put("title", title);
        json.put("subtitle", subtitle);
        json.put("applicationsTitle", applicationsTitle);
        json.put("theme", theme);
        json.put("cardLayout", cardLayout);
        json.put("showClock", showClock);
        json.put("showBranding", showBranding);
        json.put("pinnedApplicationIds", toArray(pinnedPackages));
        json.put("hiddenApplicationIds", toArray(hiddenPackages));
        return json;
    }

    static int accentColor(String theme) {
        switch (theme) {
            case "violet": return 0xFF9B8CFF;
            case "sunset": return 0xFFFFB45A;
            case "forest": return 0xFF55D6A3;
            case "rose": return 0xFFFF78A8;
            case "gold": return 0xFFF2D45C;
            case "ice": return 0xFF7CCBFF;
            case "graphite": return 0xFFC1CBD6;
            default: return 0xFF39D8F2;
        }
    }

    static int[] backgroundColors(String theme) {
        switch (theme) {
            case "violet": return new int[]{0xFF100C20, 0xFF241A46, 0xFF17152F};
            case "sunset": return new int[]{0xFF1E100D, 0xFF442515, 0xFF1E1828};
            case "forest": return new int[]{0xFF081A19, 0xFF123A31, 0xFF122320};
            case "rose": return new int[]{0xFF1B101C, 0xFF412135, 0xFF20182D};
            case "gold": return new int[]{0xFF1C190C, 0xFF433719, 0xFF222013};
            case "ice": return new int[]{0xFF0A1722, 0xFF16334A, 0xFF111E32};
            case "graphite": return new int[]{0xFF111419, 0xFF2B323B, 0xFF191C22};
            default: return new int[]{0xFF071220, 0xFF0A233E, 0xFF0B192D};
        }
    }

    static int[] heroColors(String theme) {
        switch (theme) {
            case "violet": return new int[]{0xE61E1947, 0xCC181C3D};
            case "sunset": return new int[]{0xE645291B, 0xCC302131};
            case "forest": return new int[]{0xE61A3B32, 0xCC132D2A};
            case "rose": return new int[]{0xE6422035, 0xCC2E1D31};
            case "gold": return new int[]{0xE63B3217, 0xCC292719};
            case "ice": return new int[]{0xE6173850, 0xCC152D45};
            case "graphite": return new int[]{0xE62D333D, 0xCC232833};
            default: return new int[]{0xE6112F52, 0xCC102447};
        }
    }

    private static String cleanText(String value, String fallback, int maxLength) {
        StringBuilder result = new StringBuilder();
        for (int index = 0; index < value.length() && result.length() < maxLength; index++) {
            char character = value.charAt(index);
            if (!Character.isISOControl(character)) {
                result.append(character);
            } else {
                result.append(' ');
            }
        }
        String cleaned = result.toString().replaceAll("\\s+", " ").trim();
        return cleaned.isEmpty() ? fallback : cleaned;
    }

    private static List<String> readPackages(JSONArray array) {
        if (array == null) {
            return Collections.emptyList();
        }

        List<String> packages = new ArrayList<>();
        for (int index = 0; index < array.length() && packages.size() < MAX_APPS; index++) {
            String value = array.optString(index, "").trim();
            if (PACKAGE_ID.matcher(value).matches() && !packages.contains(value)) {
                packages.add(value);
            }
        }
        return packages;
    }

    private static JSONArray toArray(List<String> values) throws JSONException {
        JSONArray array = new JSONArray();
        for (String value : values) {
            if (PACKAGE_ID.matcher(value).matches() && array.length() < MAX_APPS) {
                array.put(value);
            }
        }
        return array;
    }
}
