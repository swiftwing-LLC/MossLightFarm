import AppKit
import WebKit
import ServiceManagement

final class FarmWindow: NSWindow {
    override var canBecomeKey: Bool { true }
    override var canBecomeMain: Bool { false }
}

final class PixelBackdrop: NSView {
    var image: NSImage? { didSet { needsDisplay = true } }
    override func draw(_ dirtyRect: NSRect) {
        NSGraphicsContext.current?.imageInterpolation = .none
        image?.draw(in: bounds)
    }
}

@available(macOS 13.0, *)
final class FarmApp: NSObject, NSApplicationDelegate, WKScriptMessageHandler, WKNavigationDelegate {
    var window: FarmWindow!
    var background: NSWindow!
    var scenery: PixelBackdrop!
    var web: WKWebView!
    var statusItem: NSStatusItem!
    var timer: Timer?
    var regions: [CGRect] = []
    var quitting = false
    var preview = false
    var latestSave = "null"
    var root: URL { Bundle.main.resourceURL!.appendingPathComponent("web") }
    var saves: URL { FileManager.default.urls(for: .applicationSupportDirectory, in: .userDomainMask)[0].appendingPathComponent("MosslightFarm", isDirectory: true) }
    var saveFile: URL { saves.appendingPathComponent("save.json") }

    func applicationDidFinishLaunching(_ notification: Notification) {
        if NSRunningApplication.runningApplications(withBundleIdentifier: "games.mosslight.farm").count > 1 { NSApp.terminate(nil); return }
        NSApp.setActivationPolicy(.accessory)
        do { try FileManager.default.createDirectory(at: saves, withIntermediateDirectories: true) } catch { fail(error.localizedDescription); return }
        let content = WKUserContentController()
        content.add(self, name: "farm")
        var saved: String? = nil
        if FileManager.default.fileExists(atPath: saveFile.path) {
            for url in [saveFile, saves.appendingPathComponent("save.json.bak")] {
                if let data = try? Data(contentsOf: url), let json = try? JSONSerialization.jsonObject(with: data) as? [String: Any], json["Version"] as? Int == 1, let clean = try? JSONSerialization.data(withJSONObject: json), let text = String(data: clean, encoding: .utf8) { saved = text; break }
            }
            if saved == nil { fail("存档无法读取，原文件已保留。\nCould not read your save or backup. Original files have been preserved."); return }
        }
        latestSave = saved ?? "null"
        let initial = "window.initialSave = \(latestSave); window.initialStartup = \(SMAppService.mainApp.status == .enabled ? "true" : "false");"
        content.addUserScript(WKUserScript(source: initial, injectionTime: .atDocumentStart, forMainFrameOnly: true))
        let configuration = WKWebViewConfiguration()
        configuration.userContentController = content
        configuration.websiteDataStore = .default()
        web = WKWebView(frame: .zero, configuration: configuration)
        web.navigationDelegate = self
        web.setValue(false, forKey: "drawsBackground")
        let frame = NSScreen.main?.frame ?? CGRect(x: 0, y: 0, width: 1280, height: 800)
        background = NSWindow(contentRect: frame, styleMask: [.borderless], backing: .buffered, defer: false)
        background.level = NSWindow.Level(rawValue: Int(CGWindowLevelForKey(.desktopWindow)) + 1)
        background.collectionBehavior = [.canJoinAllSpaces, .stationary, .ignoresCycle]
        background.ignoresMouseEvents = true
        background.hasShadow = false
        scenery = PixelBackdrop(frame: CGRect(origin: .zero, size: frame.size))
        scenery.autoresizingMask = [.width, .height]
        background.contentView = scenery
        setBackground(false)
        background.orderFrontRegardless()
        window = FarmWindow(contentRect: frame, styleMask: [.borderless], backing: .buffered, defer: false)
        window.title = "Mosslight Farm"
        window.isOpaque = false
        window.backgroundColor = .clear
        window.hasShadow = false
        window.level = NSWindow.Level(rawValue: Int(CGWindowLevelForKey(.desktopIconWindow)) + 1)
        window.collectionBehavior = [.canJoinAllSpaces, .stationary, .ignoresCycle]
        window.ignoresMouseEvents = true
        window.isReleasedWhenClosed = false
        window.contentView = web
        web.loadFileURL(root.appendingPathComponent("index.html"), allowingReadAccessTo: root)
        window.orderFrontRegardless()
        makeMenu()
        timer = Timer.scheduledTimer(withTimeInterval: 0.04, repeats: true) { [weak self] _ in self?.updateHitTest() }
        NotificationCenter.default.addObserver(self, selector: #selector(screenChanged), name: NSApplication.didChangeScreenParametersNotification, object: nil)
        NSWorkspace.shared.notificationCenter.addObserver(self, selector: #selector(wake), name: NSWorkspace.didWakeNotification, object: nil)
    }

    func makeMenu() {
        statusItem = NSStatusBar.system.statusItem(withLength: NSStatusItem.variableLength)
        statusItem.button?.title = "♧"
        statusItem.button?.toolTip = "Mosslight Farm · 苔光农场"
        let menu = NSMenu()
        for (title, action) in [("设置 / Settings", #selector(settings)), ("窗口 / 桌面 · Window / Desktop", #selector(togglePreview)), ("切换显示器 / Next display", #selector(nextDisplay)), ("存档文件夹 / Save folder", #selector(openSaves)), ("保存并退出 / Save and quit", #selector(quit))] {
            let item = NSMenuItem(title: title, action: action, keyEquivalent: "")
            item.target = self
            menu.addItem(item)
        }
        statusItem.menu = menu
    }
    func updateHitTest() {
        guard window != nil else { return }
        if preview { window.ignoresMouseEvents = false; return }
        if NSEvent.pressedMouseButtons != 0 { return }
        let p = NSEvent.mouseLocation, f = window.frame
        let local = CGPoint(x: (p.x-f.minX)/f.width, y: 1-(p.y-f.minY)/f.height)
        window.ignoresMouseEvents = !f.contains(p) || !regions.contains(where: { $0.contains(local) })
    }
    func setBackground(_ night: Bool) {
        scenery?.image = NSImage(contentsOf: root.appendingPathComponent("sprites/meadow\(night ? "-night" : "").png"))
    }
    func js(_ source: String) { web?.evaluateJavaScript(source, completionHandler: nil) }
    func reply(_ name: String, _ value: Any) {
        if let data = try? JSONSerialization.data(withJSONObject: value, options: [.fragmentsAllowed]), let string = String(data: data, encoding: .utf8) { js("\(name)(\(string))") }
    }
    func userContentController(_ userContentController: WKUserContentController, didReceive message: WKScriptMessage) {
        guard message.frameInfo.isMainFrame, let body = message.body as? [String: Any], let type = body["type"] as? String else { return }
        switch type {
        case "regions":
            if let list = body["data"] as? [[Double]], list.count < 300 { regions = list.filter { $0.count == 4 && $0.allSatisfy { $0.isFinite } }.map { CGRect(x: $0[0], y: $0[1], width: $0[2], height: $0[3]) } }
        case "background": setBackground(body["data"] as? Bool ?? false)
        case "save":
            do {
                guard let object = body["data"] as? [String: Any], object["Version"] as? Int == 1 else { return }
                let bytes = try JSONSerialization.data(withJSONObject: object, options: [.sortedKeys])
                if FileManager.default.fileExists(atPath: saveFile.path) { let old = try Data(contentsOf: saveFile); try old.write(to: saves.appendingPathComponent("save.json.bak"), options: .atomic) }
                try bytes.write(to: saveFile, options: .atomic)
                latestSave = String(data: bytes, encoding: .utf8) ?? latestSave
                let controller = web.configuration.userContentController
                controller.removeAllUserScripts()
                controller.addUserScript(WKUserScript(source: "window.initialSave = \(latestSave); window.initialStartup = \(SMAppService.mainApp.status == .enabled ? "true" : "false");", injectionTime: .atDocumentStart, forMainFrameOnly: true))
                reply("window.nativeSaveResult", "")
            } catch { reply("window.nativeSaveResult", error.localizedDescription) }
        case "startup":
            do {
                if body["data"] as? Bool == true { try SMAppService.mainApp.register() } else { try SMAppService.mainApp.unregister() }
                let status = SMAppService.mainApp.status
                let error = status == .requiresApproval ? "请在系统设置的登录项中允许 Mosslight。 / Allow Mosslight in System Settings > Login Items." : ""
                if let data = try? JSONSerialization.data(withJSONObject: error, options: [.fragmentsAllowed]), let text = String(data: data, encoding: .utf8) { js("window.nativeStartupResult(\(status == .enabled ? "true" : "false"),\(text))") }
            } catch { reply("window.nativeSaveResult", error.localizedDescription) }
        case "repair": screenChanged(); wake()
        default: break
        }
    }
    func webView(_ webView: WKWebView, decidePolicyFor navigationAction: WKNavigationAction, decisionHandler: @escaping (WKNavigationActionPolicy) -> Void) {
        guard let url = navigationAction.request.url, url.isFileURL, url.standardizedFileURL.path.hasPrefix(root.standardizedFileURL.path + "/") else { decisionHandler(.cancel); return }
        decisionHandler(.allow)
    }
    func webViewWebContentProcessDidTerminate(_ webView: WKWebView) { webView.reload() }
    @objc func settings() { js("window.nativeOpenSettings()"); window.orderFrontRegardless() }
    @objc func openSaves() { NSWorkspace.shared.open(saves) }
    @objc func wake() { js("game.tick(); if(typeof refresh==='function')refresh();"); background.orderFrontRegardless(); window.orderFrontRegardless() }
    @objc func screenChanged() { if !preview, let screen = window.screen ?? NSScreen.main { window.setFrame(screen.frame, display: true); background.setFrame(screen.frame, display: true) } }
    @objc func nextDisplay() { guard !preview, NSScreen.screens.count > 1 else { return }; let screens = NSScreen.screens; let index = screens.firstIndex(where: { $0 == window.screen }) ?? 0; window.setFrame(screens[(index+1)%screens.count].frame, display: true); background.setFrame(window.frame, display: true) }
    @objc func togglePreview() {
        preview.toggle()
        if preview { window.level = .normal; window.styleMask = [.titled, .resizable, .miniaturizable]; window.setContentSize(NSSize(width: 1100, height: 720)); window.center(); window.makeKeyAndOrderFront(nil); NSApp.activate(ignoringOtherApps: true) }
        else { window.styleMask = [.borderless]; window.level = NSWindow.Level(rawValue: Int(CGWindowLevelForKey(.desktopIconWindow))+1); screenChanged() }
    }
    @objc func quit() { NSApp.terminate(nil) }
    func applicationShouldTerminate(_ sender: NSApplication) -> NSApplication.TerminateReply {
        if quitting { return .terminateNow }
        quitting = true
        web?.evaluateJavaScript("window.saveFarm()") { _, _ in NSApp.reply(toApplicationShouldTerminate: true) }
        DispatchQueue.main.asyncAfter(deadline: .now()+2) { NSApp.reply(toApplicationShouldTerminate: true) }
        return web == nil ? .terminateNow : .terminateLater
    }
    func fail(_ message: String) { let alert = NSAlert(); alert.messageText = "Mosslight Farm"; alert.informativeText = message; alert.runModal(); NSApp.terminate(nil) }
}

if #available(macOS 13.0, *) {
    let app = NSApplication.shared
    let delegate = FarmApp()
    app.delegate = delegate
    app.run()
}
