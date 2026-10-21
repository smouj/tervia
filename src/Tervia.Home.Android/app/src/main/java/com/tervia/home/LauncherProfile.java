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
        THEMES = Collections.unmodifiableSet(themes);
    }

    final String title;
    final String subtitle;
    final String theme;
    final boolean showClock;
    final List<String> pinnedPackages;
    final List<String> hiddenPackages;

    private LauncherProfile(
            String title,
            String subtitle,
            String theme,
            boolean showClock,
            List<String> pinnedPackages,
            List<String> hiddenPackages) {
        this.title = title;
        this.subtitle = subtitle;
        this.theme = theme;
        this.showClock = showClock;
        this.pinnedPackages = Collections.unmodifiableList(pinnedPackages);
        this.hiddenPackages = Collections.unmodifiableList(hiddenPackages);
    }

    static LauncherProfile defaults() {
        return new LauncherProfile(DEFAULT_TITLE, DEFAULT_SUBTITLE, "ocean", true,
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

        return new LauncherProfile(
                title,
                subtitle,
                theme,
                json.optBoolean("showClock", true),
                readPackages(json.optJSONArray("pinnedApplicationIds")),
                readPackages(json.optJSONArray("hiddenApplicationIds")));
    }

    JSONObject toJson() throws JSONException {
        JSONObject json = new JSONObject();
        json.put("version", 1);
        json.put("title", title);
        json.put("subtitle", subtitle);
        json.put("theme", theme);
        json.put("showClock", showClock);
        json.put("pinnedApplicationIds", toArray(pinnedPackages));
        json.put("hiddenApplicationIds", toArray(hiddenPackages));
        return json;
    }

    static int accentColor(String theme) {
        switch (theme) {
            case "violet": return 0xFF9B8CFF;
            case "sunset": return 0xFFFFB45A;
            case "forest": return 0xFF55D6A3;
            default: return 0xFF39D8F2;
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
