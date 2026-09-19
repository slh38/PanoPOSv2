fn main() {
    println!("cargo:rerun-if-changed=../desktop.config.json");
    let config: serde_json::Value = serde_json::from_str(
        &std::fs::read_to_string("../desktop.config.json").expect("Desktop config is missing"),
    ).expect("Desktop config is invalid");
    let base = url::Url::parse(config["apiBaseUrl"].as_str().expect("API base URL is missing"))
        .expect("API base URL is invalid");
    assert!(matches!(base.scheme(), "http" | "https") && base.host_str().is_some()
        && base.username().is_empty() && base.password().is_none()
        && base.path() == "/" && base.query().is_none() && base.fragment().is_none(),
        "API base URL must be an HTTP(S) origin");
    // Compile the native HTTP allowlist from the same configuration as the frontend.
    let capability = serde_json::json!({
        "identifier": "desktop-api",
        "windows": ["main"],
        "permissions": [{
            "identifier": "http:default",
            "allow": [{ "url": format!("{}/api/v1/*", base.origin().ascii_serialization()) }]
        }]
    });
    std::fs::create_dir_all("gen/capabilities").expect("Cannot create capability directory");
    std::fs::write("gen/capabilities/desktop-api.json", capability.to_string())
        .expect("Cannot write API capability");
    tauri_build::try_build(
        tauri_build::Attributes::new().capabilities_path_pattern("gen/capabilities/*.json"),
    ).expect("Tauri build failed");
}
