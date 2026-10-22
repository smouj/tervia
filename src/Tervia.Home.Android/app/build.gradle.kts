plugins {
    id("com.android.application")
}

val terviaHomeVersionName = providers.environmentVariable("TERVIA_HOME_VERSION_NAME")
    .orElse("0.2.0-alpha")
    .get()
val terviaHomeVersionCode = providers.environmentVariable("TERVIA_HOME_VERSION_CODE")
    .map { it.toInt() }
    .getOrElse(1)

android {
    namespace = "com.tervia.home"
    compileSdk = 36

    defaultConfig {
        applicationId = "com.tervia.home"
        minSdk = 23
        targetSdk = 36
        versionCode = terviaHomeVersionCode
        versionName = terviaHomeVersionName
    }

    signingConfigs {
        create("release") {
            val storePath = System.getenv("TERVIA_HOME_KEYSTORE_PATH")
            if (!storePath.isNullOrBlank()) {
                storeFile = file(storePath)
                storePassword = System.getenv("TERVIA_HOME_KEYSTORE_PASSWORD")
                keyAlias = System.getenv("TERVIA_HOME_KEY_ALIAS")
                keyPassword = System.getenv("TERVIA_HOME_KEY_PASSWORD")
            }
        }
    }

    buildTypes {
        release {
            isMinifyEnabled = false
            signingConfig = signingConfigs.getByName("release")
        }
    }
}
