import org.gradle.crypto.checksum.Checksum
import java.io.FileInputStream
import java.util.*

plugins {
    java
    signing
    distribution
    id("org.gradle.crypto.checksum") version "1.4.0"
    id("com.diffplug.spotless") version "6.12.0"
    id("org.omegat.gradle") version "2.0.0-rc2"
    id("com.palantir.git-version") version "3.0.0" apply false
}

version = "1.0.0"


java {
    toolchain {
        languageVersion.set(JavaLanguageVersion.of(8))
    }
}

omegat {
    version("5.8.0") // target java version is 8
    pluginClass("helsinki_nlp.opuscat.omegat_plugin.OpusCatPlugin")
    packIntoJarFileFilter = {it.exclude("META-INF/**/*", "module-info.class", "kotlin/**/*")}
}


repositories {
    mavenCentral()
}

dependencies {
    implementation("com.fasterxml.jackson.core:jackson-core:2.13.4")
    implementation("com.fasterxml.jackson.core:jackson-annotations:2.13.4")
    implementation("com.fasterxml.jackson.core:jackson-databind:2.13.4")
    implementation("com.github.ben-manes.caffeine:caffeine:2.9.3")
    implementation("com.github.ben-manes.caffeine:jcache:2.9.3")
    testImplementation("org.junit.jupiter:junit-jupiter:5.8.1")
    testImplementation("com.github.tomakehurst:wiremock-jre8-standalone:2.35.0")
}

tasks.withType<Test>().configureEach {
    useJUnitPlatform()
}

distributions {
    main {
        contents {
            from(tasks["jar"], "README.md", "COPYING", "CHANGELOG.md")
        }
    }
}

val signKey = listOf("signingKey", "signing.keyId", "signing.gnupg.keyName").find {project.hasProperty(it)}
tasks.withType<Sign> {
    onlyIf { signKey != null }
}

signing {
    when (signKey) {
        "signingKey" -> {
            val signingKey: String? by project
            val signingPassword: String? by project
            useInMemoryPgpKeys(signingKey, signingPassword)
        }
        "signing.keyId" -> {/* do nothing */
        }
        "signing.gnupg.keyName" -> {
            useGpgCmd()
        }
    }
    sign(tasks.distZip.get())
    sign(tasks.jar.get())
}

val jar by tasks.getting(Jar::class) {
    duplicatesStrategy = DuplicatesStrategy.INCLUDE
}



tasks.register<Checksum>("createChecksums") {
    dependsOn(tasks.distZip)
    inputFiles.setFrom(listOf(tasks.jar.get(), tasks.distZip.get()))
    outputDirectory.set(layout.buildDirectory.dir("distributions"))
    checksumAlgorithm.set(Checksum.Algorithm.SHA512)
    appendFileNameToChecksum.set(true)
}
