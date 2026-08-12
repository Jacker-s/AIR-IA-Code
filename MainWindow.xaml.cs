using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Media = System.Windows.Media;
using Forms = System.Windows.Forms;

namespace AirCodeNative;

public partial class MainWindow : Window
{
    const string ProductName = "AIR IA Code";
    const string DeveloperName = "Codename Jackers";
    record Model(string Name, string Detail, string File, string Url, string GpuLayers, string Size, string Speed, string Category, string Fit, string? LocalPath = null, bool Detected = false) { public override string ToString() => Name; }
    readonly string root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AirCodeLocal");
    readonly ObservableCollection<string> projects = new();
    readonly List<Dictionary<string, string>> history = new();
    readonly List<Model> models = new()
    {
        new("Qwen Coder 1.5B", "Resposta quase instantânea para completar código e tarefas simples.", "qwen2.5-coder-1.5b-instruct-q4_k_m.gguf", "https://huggingface.co/Qwen/Qwen2.5-Coder-1.5B-Instruct-GGUF/resolve/main/qwen2.5-coder-1.5b-instruct-q4_k_m.gguf?download=true", "all", "1,1 GB", "35–65 tok/s", "ULTRARRÁPIDO", "Excelente"),
        new("Qwen Coder 3B", "Ótimo equilíbrio para edições, scripts, explicações e Android básico.", "qwen2.5-coder-3b-instruct-q4_k_m.gguf", "https://huggingface.co/Qwen/Qwen2.5-Coder-3B-Instruct-GGUF/resolve/main/qwen2.5-coder-3b-instruct-q4_k_m.gguf?download=true", "all", "2,0 GB", "28–50 tok/s", "RÁPIDO", "Excelente"),
        new("Llama 3.2 3B", "Conversa geral rápida, escrita, resumo e perguntas do dia a dia.", "Llama-3.2-3B-Instruct-Q4_K_M.gguf", "https://huggingface.co/bartowski/Llama-3.2-3B-Instruct-GGUF/resolve/main/Llama-3.2-3B-Instruct-Q4_K_M.gguf?download=true", "all", "2,0 GB", "28–48 tok/s", "CHAT RÁPIDO", "Excelente"),
        new("Llama 3.2 3B Uncensored", "Modelo geral compacto com menos recusas e boa velocidade.", "Llama-3.2-3B-Instruct-uncensored-Q4_K_M.gguf", "https://huggingface.co/bartowski/Llama-3.2-3B-Instruct-uncensored-GGUF/resolve/main/Llama-3.2-3B-Instruct-uncensored-Q4_K_M.gguf?download=true", "all", "2,2 GB", "26–45 tok/s", "SEM FILTROS", "Excelente"),
        new("Phi-3 Mini 3.8B", "Assistente pequeno para raciocínio, estudo, texto e tarefas gerais.", "Phi-3-mini-4k-instruct-q4.gguf", "https://huggingface.co/microsoft/Phi-3-mini-4k-instruct-gguf/resolve/main/Phi-3-mini-4k-instruct-q4.gguf?download=true", "all", "2,2 GB", "25–45 tok/s", "CHAT RÁPIDO", "Excelente"),
        new("Gemma 3 4B", "Modelo geral moderno para conversa, análise e produção de conteúdo.", "gemma-3-4b-it-Q4_K_M.gguf", "https://huggingface.co/unsloth/gemma-3-4b-it-GGUF/resolve/main/gemma-3-4b-it-Q4_K_M.gguf?download=true", "all", "2,5 GB", "22–40 tok/s", "GERAL", "Excelente"),
        new("Qwen Coder 7B oficial", "Recomendado para programação diária, refatoração e criação de aplicativos.", "qwen2.5-coder-7b-instruct-q4_k_m.gguf", "https://huggingface.co/Qwen/Qwen2.5-Coder-7B-Instruct-GGUF/resolve/main/qwen2.5-coder-7b-instruct-q4_k_m.gguf?download=true", "all", "4,7 GB", "18–35 tok/s", "RECOMENDADO", "Excelente"),
        new("Qwen Coder 7B Uncensored", "Variante com menos recusas, focada em código e automação local.", "qwen2.5-coder-7b-instruct-uncensored-q4_k_m.gguf", "https://huggingface.co/BlossomsAI/Qwen2.5-Coder-7B-Instruct-Uncensored-GGUF/resolve/main/q4_k_m.gguf?download=true", "all", "4,7 GB", "18–35 tok/s", "SEM FILTROS", "Excelente"),
        new("Llama 3.1 8B Uncensored", "Chat geral, planejamento e código com poucas recusas.", "Llama-3.1-8B-Uncensored.Q4_K_M.gguf", "https://huggingface.co/mradermacher/Llama-3.1-8B-Uncensored-GGUF/resolve/main/Llama-3.1-8B-Uncensored.Q4_K_M.gguf?download=true", "all", "5,0 GB", "14–28 tok/s", "SEM FILTROS", "Muito bom"),
        new("Mistral 7B Instruct", "Assistente geral equilibrado para conversa, escrita e raciocínio.", "Mistral-7B-Instruct-v0.3-Q4_K_M.gguf", "https://huggingface.co/bartowski/Mistral-7B-Instruct-v0.3-GGUF/resolve/main/Mistral-7B-Instruct-v0.3-Q4_K_M.gguf?download=true", "all", "4,4 GB", "17–32 tok/s", "GERAL", "Excelente"),
        new("Hermes 3 Llama 8B", "Chat geral versátil, instruções complexas e uso de ferramentas.", "Hermes-3-Llama-3.1-8B.Q4_K_M.gguf", "https://huggingface.co/NousResearch/Hermes-3-Llama-3.1-8B-GGUF/resolve/main/Hermes-3-Llama-3.1-8B.Q4_K_M.gguf?download=true", "all", "4,9 GB", "14–27 tok/s", "GERAL", "Muito bom"),
        new("Gemma 2 9B", "Boa qualidade em conversa, conhecimento e produção de textos longos.", "gemma-2-9b-it-Q4_K_M.gguf", "https://huggingface.co/bartowski/gemma-2-9b-it-GGUF/resolve/main/gemma-2-9b-it-Q4_K_M.gguf?download=true", "all", "5,8 GB", "12–24 tok/s", "GERAL", "Muito bom"),
        new("Qwen Coder 14B oficial", "Mais precisão em projetos grandes; usa GPU e RAM simultaneamente.", "qwen2.5-coder-14b-instruct-q4_k_m.gguf", "https://huggingface.co/Qwen/Qwen2.5-Coder-14B-Instruct-GGUF/resolve/main/qwen2.5-coder-14b-instruct-q4_k_m.gguf?download=true", "28", "9,0 GB", "7–15 tok/s", "AVANÇADO", "Parcial"),
        new("Qwen Coder 14B Uncensored", "Mais capacidade e menos recusas; requer pelo menos 16 GB de RAM.", "qwen2.5-coder-14b-instruct-uncensored-q4_k_m.gguf", "https://huggingface.co/BlossomsAI/Qwen2.5-Coder-14B-Instruct-Uncensored-GGUF/resolve/main/q4_k_m.gguf?download=true", "28", "9,0 GB", "7–15 tok/s", "SEM FILTROS", "Parcial"),
        new("DeepSeek Coder V2 Lite 16B", "Especialista MoE em código; forte, mas mais lento nesta placa.", "DeepSeek-Coder-V2-Lite-Instruct-Q4_K_M.gguf", "https://huggingface.co/bartowski/DeepSeek-Coder-V2-Lite-Instruct-GGUF/resolve/main/DeepSeek-Coder-V2-Lite-Instruct-Q4_K_M.gguf?download=true", "24", "10,4 GB", "5–11 tok/s", "AVANÇADO", "Parcial"),
        new("Dolphin Mistral 24B", "Raciocínio geral sem filtros; indicado quando qualidade vale mais que velocidade.", "Dolphin3.0-Mistral-24B.Q4_K_M.gguf", "https://huggingface.co/mradermacher/Dolphin3.0-Mistral-24B-GGUF/resolve/main/Dolphin3.0-Mistral-24B.Q4_K_M.gguf?download=true", "18", "14 GB", "2–6 tok/s", "GRANDE", "Lento"),
        new("Qwen Coder 32B", "Maior qualidade do catálogo, exige bastante RAM e carregamento parcial.", "Qwen2.5-Coder-32B-Instruct-Q4_K_M.gguf", "https://huggingface.co/Qwen/Qwen2.5-Coder-32B-Instruct-GGUF/resolve/main/Qwen2.5-Coder-32B-Instruct-Q4_K_M.gguf?download=true", "12", "20 GB", "2–5 tok/s", "GRANDE", "Lento")
    };
    Process? server;
    Process? mediaServer;
    string? runningModelPath;
    string? mediaResultPath;
    readonly Dictionary<int, Process> agentProcesses = new();
    readonly Dictionary<int, StringBuilder> agentProcessLogs = new();
    CancellationTokenSource? downloadCancellation;
    Model? downloadingModel;
    CancellationTokenSource? generationCancellation;
    bool isSending;
    string? activeProject;
    string? currentFile;
    bool loadingSettings = true;
    bool refreshingModelSelectors;
    string currentModelFilter = "all";
    sealed class SavedState { public List<string> Projects { get; set; } = new(); public Dictionary<string, List<Dictionary<string, string>>> Chats { get; set; } = new(); public Dictionary<string, string> ProjectMemories { get; set; } = new(); public Dictionary<string, List<string>> ProjectActions { get; set; } = new(); public string LastProject { get; set; } = ""; public string Context { get; set; } = "8192"; public string GpuLayers { get; set; } = "auto"; public string Temperature { get; set; } = "0.30"; public int MaxTokens { get; set; } = 1800; public string PerformanceMode { get; set; } = "Automático (recomendado)"; public bool ConfirmCommands { get; set; } = false; public bool FullAccessConfigured { get; set; } public bool AutoStart { get; set; } public string DefaultModelPath { get; set; } = ""; public string MediaEndpoint { get; set; } = "http://127.0.0.1:8188"; public string MediaDevice { get; set; } = "AMD DirectML"; public string ComfyPath { get; set; } = ""; public string ImageWorkflow { get; set; } = ""; public string VideoWorkflow { get; set; } = ""; }
    SavedState saved = new();
    string StateFile => Path.Combine(root, "settings.json");
    string ServerPidFile => Path.Combine(root, "llama-server.pid");
    string ModelsPath => Path.Combine(root, "modelos");

    public MainWindow()
    {
        InitializeComponent();
        LoadState();
        SetupMediaButton.Content = "Configurar " + saved.MediaDevice.Split(' ')[0];
        if (!saved.FullAccessConfigured) { saved.FullAccessConfigured = true; saved.ConfirmCommands = false; ConfirmCommandsBox.IsChecked = false; }
        loadingSettings = true;
        RefreshModelLists();
        ModelsBox.SelectedItem = models[0];
        loadingSettings = false;
        RefreshProjectsSidebar();
        if (!Directory.Exists(saved.LastProject)) saved.LastProject = projects.LastOrDefault(Directory.Exists) ?? "";
        if (Directory.Exists(saved.LastProject)) OpenProject(saved.LastProject);
        SaveState();
        Loaded += async (_, _) => await InitializeModelsAsync();
    }
    Model Selected => (Model)ModelsBox.SelectedItem;
    string ResolvedPath(Model model) => model.LocalPath ?? Path.Combine(ModelsPath, model.File);
    static bool SamePath(string? left, string? right) => !string.IsNullOrWhiteSpace(left) && !string.IsNullOrWhiteSpace(right) && string.Equals(Path.GetFullPath(left), Path.GetFullPath(right), StringComparison.OrdinalIgnoreCase);
    static bool IsUsableFile(string path) { try { return File.Exists(path) && new FileInfo(path).Length > 100 * 1024 * 1024; } catch { return false; } }
    bool IsInstalled(Model model) => IsUsableFile(ResolvedPath(model));
    void SetTab(UIElement page, string title, string subtitle) { ChatPage.Visibility = ModelsPage.Visibility = MediaPage.Visibility = ProjectsPage.Visibility = TerminalPage.Visibility = SettingsPage.Visibility = Visibility.Collapsed; page.Visibility = Visibility.Visible; PageTitle.Text = title; PageSubtitle.Text = subtitle; UpdateNavigation(page); }
    void UpdateNavigation(UIElement activePage)
    {
        var activeLabel = activePage == ModelsPage ? "◇  Modelos" : activePage == MediaPage ? "✦  Mídia IA" : activePage == ProjectsPage ? "▣  Workspace" : activePage == TerminalPage ? "›_  Terminal" : activePage == SettingsPage ? "⚙  Configurações" : "＋  Novo chat";
        string[] navigationLabels = ["＋  Novo chat", "◇  Modelos", "✦  Mídia IA", "▣  Workspace", "›_  Terminal", "⚙  Configurações"];
        foreach (var button in FindVisualChildren<System.Windows.Controls.Button>(this).Where(button => button.Content is string text && navigationLabels.Contains(text)))
        {
            var selected = Equals(button.Content, activeLabel); button.Background = new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString(selected ? "#293632" : "Transparent")); button.Foreground = new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString(selected ? "#F3F6F5" : "#D1D7D5"));
        }
    }
    static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var index = 0; index < Media.VisualTreeHelper.GetChildrenCount(parent); index++) { var child = Media.VisualTreeHelper.GetChild(parent, index); if (child is T match) yield return match; foreach (var descendant in FindVisualChildren<T>(child)) yield return descendant; }
    }
    void ChatTab_Click(object sender, RoutedEventArgs e)
    {
        activeProject = null; saved.LastProject = ""; SidebarProjects.SelectedIndex = -1; history.Clear(); saved.Chats["__general"] = new();
        ActiveProjectText.Text = "Chat independente"; ProjectAccessText.Text = "Sem projeto · conversa privada"; ProjectAccessText.Foreground = new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString("#A8AFB3"));
        SaveState(); RenderChat(); SetTab(ChatPage, "Novo chat", "Conversa local independente · nenhum projeto conectado"); PromptBox.Focus();
    }
    void ModelsTab_Click(object sender, RoutedEventArgs e) => SetTab(ModelsPage, "Modelos locais", "Baixe, inicie e acompanhe seu modelo por aqui.");
    async void MediaTab_Click(object sender, RoutedEventArgs e) { SetTab(MediaPage, "Estúdio de mídia", "Crie imagens e vídeos localmente; o AirCode prepara tudo sozinho."); MediaSettingsScroll.ScrollToTop(); await CheckMediaEngine(false); }
    void ProjectsTab_Click(object sender, RoutedEventArgs e) { if (activeProject is null) { AddProject_Click(sender, e); return; } SetTab(ProjectsPage, Path.GetFileName(activeProject), "Arquivos e editor do projeto ativo."); }
    void TerminalTab_Click(object sender, RoutedEventArgs e) => SetTab(TerminalPage, "Terminal", activeProject is null ? "Selecione um projeto antes de executar comandos." : activeProject);
    void SettingsTab_Click(object sender, RoutedEventArgs e) => SetTab(SettingsPage, "Configurações", "Controle de desempenho, contexto e segurança.");
    void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { if (e.ClickCount == 2) ToggleMaximize(); else if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
    void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    void Maximize_Click(object sender, RoutedEventArgs e) => ToggleMaximize();
    void Close_Click(object sender, RoutedEventArgs e) => Close();
    void ToggleMaximize() => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    async Task InitializeModelsAsync()
    {
        InstalledCountText.Text = "Procurando modelos sem bloquear a interface…";
        var files = await Task.Run(FindInstalledModelFiles);
        ApplyInstalledModels(files);
        loadingSettings = true; RefreshModelLists();
        var preferredFile = Path.GetFileName(saved.DefaultModelPath);
        var preferred = models.FirstOrDefault(m => IsInstalled(m) && (SamePath(ResolvedPath(m), saved.DefaultModelPath) || string.Equals(m.File, preferredFile, StringComparison.OrdinalIgnoreCase)));
        ModelsBox.SelectedItem = preferred ?? models.FirstOrDefault(IsInstalled) ?? models[0];
        DefaultModelBox.SelectedItem = preferred; loadingSettings = false;
        ModelsBox_SelectionChanged(this, null!);
        if (saved.AutoStart && preferred is not null) { await Task.Delay(250); StartModel(preferred, false); }
    }
    List<string> FindInstalledModelFiles()
    {
        Directory.CreateDirectory(ModelsPath);
        var roots = new[]
        {
            ModelsPath,
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Desktop", "Projetos", "Airllm", "modelos"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache", "huggingface", "hub")
        };
        return roots.Where(Directory.Exists).SelectMany(path =>
        {
            try { return Directory.EnumerateFiles(path, "*.gguf", SearchOption.AllDirectories); }
            catch { return Enumerable.Empty<string>(); }
        }).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }
    void ApplyInstalledModels(List<string> files)
    {
        foreach (var group in files.GroupBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase))
        {
            var best = group.OrderByDescending(path => { try { return new FileInfo(path).Length; } catch { return 0; } }).First();
            var index = models.FindIndex(m => string.Equals(m.File, group.Key, StringComparison.OrdinalIgnoreCase));
            if (index >= 0) models[index] = models[index] with { LocalPath = best };
            else
            {
                var info = new FileInfo(best);
                models.Add(new Model(Path.GetFileNameWithoutExtension(best), "Modelo GGUF detectado automaticamente no computador.", info.Name, "", "all", FormatBytes(info.Length), "Depende do modelo", "LOCAL", "A verificar", best, true));
            }
        }
    }
    void RefreshModelLists()
    {
        ApplyModelFilter();
        var installed = models.Where(IsInstalled).ToList(); var previousChatModel = ChatModelBox.SelectedItem as Model;
        refreshingModelSelectors = true;
        DefaultModelBox.ItemsSource = installed; DefaultModelBox.DisplayMemberPath = "Name";
        DefaultModelBox.SelectedItem = installed.FirstOrDefault(m => SamePath(ResolvedPath(m), saved.DefaultModelPath));
        ChatModelBox.ItemsSource = installed;
        ChatModelBox.SelectedItem = installed.FirstOrDefault(m => SamePath(ResolvedPath(m), runningModelPath)) ?? (previousChatModel is not null && installed.Contains(previousChatModel) ? previousChatModel : DefaultModelBox.SelectedItem as Model ?? installed.FirstOrDefault());
        refreshingModelSelectors = false;
        ChatModelBox.ToolTip = installed.Count == 0 ? "Baixe um modelo para selecioná-lo no chat" : "Trocar o modelo usado neste chat";
        InstalledCountText.Text = $"{models.Count(IsInstalled)} instalados · {models.Count} disponíveis";
    }
    void ApplyModelFilter()
    {
        var selected = ModelsBox.SelectedItem as Model;
        var query = ModelSearchBox?.Text?.Trim() ?? "";
        IEnumerable<Model> filtered = models;
        if (!string.IsNullOrWhiteSpace(query)) filtered = filtered.Where(m => $"{m.Name} {m.Detail} {m.Category} {m.Size}".Contains(query, StringComparison.OrdinalIgnoreCase));
        filtered = currentModelFilter switch
        {
            "fast" => filtered.Where(m => m.Category is "ULTRARRÁPIDO" or "RÁPIDO" or "CHAT RÁPIDO" or "RECOMENDADO"),
            "general" => filtered.Where(m => m.Category is "GERAL" or "CHAT RÁPIDO"),
            "installed" => filtered.Where(IsInstalled),
            "advanced" => filtered.Where(m => m.Category is "AVANÇADO" or "GRANDE"),
            _ => filtered
        };
        var visible = filtered.ToList(); ModelsBox.ItemsSource = visible;
        ModelsBox.SelectedItem = selected is not null && visible.Contains(selected) ? selected : visible.FirstOrDefault();
    }
    void ModelSearchBox_TextChanged(object sender, TextChangedEventArgs e) { if (ModelsBox is not null) ApplyModelFilter(); }
    void ModelFilter_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button selected) return;
        currentModelFilter = selected.Tag?.ToString() ?? "all";
        if (selected.Parent is System.Windows.Controls.Panel panel) foreach (var button in panel.Children.OfType<System.Windows.Controls.Button>()) { button.Background = new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString(button == selected ? "#393050" : "#1C2023")); button.BorderBrush = new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString(button == selected ? "#6958A0" : "#34393D")); }
        ApplyModelFilter();
    }
    void ModelsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ModelsBox.SelectedItem is not Model m) return;
        var path = ResolvedPath(m);
        var exists = File.Exists(path);
        var installed = IsInstalled(m);
        var size = exists ? FormatBytes(new FileInfo(path).Length) : "0 B";
        var partial = path + ".part"; var partialExists = File.Exists(partial);
        SelectedModelTitle.Text = m.Name;
        ModelFitBadge.Text = $"RX 5700 XT · {m.Fit}";
        ModelInfo.Text = $"{m.Detail}\n\nTamanho: {m.Size}   ·   Velocidade estimada: {m.Speed}" + (installed ? $"\n\n● Instalado · {size}\n{path}" : partialExists ? $"\n\n◐ Download pausado · {FormatBytes(new FileInfo(partial).Length)}" : "\n\n○ Disponível para download");
        DownloadButton.Content = downloadCancellation is not null ? (downloadingModel == m ? "Baixando…" : "Download em andamento") : installed ? "Baixar novamente" : partialExists ? "Retomar" : "Baixar";
        DownloadButton.IsEnabled = downloadCancellation is null;
        StartButton.IsEnabled = installed;
        DefaultButton.IsEnabled = installed;
        RemoveModelButton.IsEnabled = installed || partialExists;
        RemoveModelButton.Content = installed ? "Remover arquivo" : partialExists ? "Descartar parcial" : "Nada para remover";
        var isDefault = SamePath(path, saved.DefaultModelPath);
        DefaultButton.Content = isDefault ? "✓ Modelo padrão" : "Definir como padrão";
    }
    void AddCustomModel_Click(object sender, RoutedEventArgs e)
    {
        if (!Uri.TryCreate(CustomUrlBox.Text.Trim(), UriKind.Absolute, out var uri) || !uri.AbsolutePath.EndsWith(".gguf", StringComparison.OrdinalIgnoreCase)) { System.Windows.MessageBox.Show("Informe uma URL direta de um arquivo .gguf.", "AirCode"); return; }
        var file = Path.GetFileName(uri.AbsolutePath); var custom = new Model("Modelo personalizado", "GGUF informado pelo usuário; desempenho depende do tamanho e da quantização.", file, uri.ToString(), "all", "A verificar", "A estimar", "PERSONALIZADO", "A verificar");
        models.Add(custom); RefreshModelLists(); ModelsBox.SelectedItem = custom; Log("Modelo personalizado adicionado ao catálogo.");
    }
    void Log(string text) => Dispatcher.Invoke(() => { LogBox.Text = (text + "\n" + LogBox.Text)[..Math.Min(6000, text.Length + 1 + LogBox.Text.Length)]; });
    async void Download_Click(object sender, RoutedEventArgs e)
    {
        if (downloadCancellation is not null) { downloadCancellation.Cancel(); return; }
        Directory.CreateDirectory(ModelsPath); var m = Selected;
        if (string.IsNullOrWhiteSpace(m.Url)) { System.Windows.MessageBox.Show("Este modelo foi detectado localmente e não possui uma URL de download.", "AirCode"); return; }
        var target = Path.Combine(ModelsPath, m.File); var partialTarget = target + ".part"; downloadCancellation = new CancellationTokenSource(); downloadingModel = m;
        var drive = new DriveInfo(Path.GetPathRoot(ModelsPath)!); if (drive.AvailableFreeSpace < 2L * 1024 * 1024 * 1024) { downloadCancellation.Dispose(); downloadCancellation = null; System.Windows.MessageBox.Show("Há menos de 2 GB livres no disco. Libere espaço antes de baixar um modelo.", "AirCode"); return; }
        DownloadButton.Content = "Baixando…"; DownloadButton.IsEnabled = false; DownloadStatusPanel.Visibility = Visibility.Visible;
        try
        {
            var existing = File.Exists(partialTarget) ? new FileInfo(partialTarget).Length : 0L;
            using var client = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
            using var request = new HttpRequestMessage(HttpMethod.Get, m.Url);
            if (existing > 0) request.Headers.Range = new RangeHeaderValue(existing, null);
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, downloadCancellation.Token);
            response.EnsureSuccessStatusCode();
            var resumed = response.StatusCode == System.Net.HttpStatusCode.PartialContent;
            if (!resumed) existing = 0;
            var incoming = response.Content.Headers.ContentLength ?? 0;
            var total = existing + incoming;
            await using var input = await response.Content.ReadAsStreamAsync(downloadCancellation.Token);
            await using var output = new FileStream(partialTarget, resumed ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read, 1024 * 1024, true);
            var buffer = new byte[1024 * 1024]; long downloaded = existing; var clock = Stopwatch.StartNew(); long lastBytes = downloaded; long lastMs = 0;
            Log((resumed ? "Retomando " : "Iniciando ") + "download de " + m.Name + "…");
            while (true)
            {
                var read = await input.ReadAsync(buffer, downloadCancellation.Token); if (read == 0) break;
                await output.WriteAsync(buffer.AsMemory(0, read), downloadCancellation.Token); downloaded += read;
                if (clock.ElapsedMilliseconds - lastMs >= 250 || downloaded == total)
                {
                    var elapsed = Math.Max(1, clock.ElapsedMilliseconds - lastMs); var speed = (downloaded - lastBytes) * 1000d / elapsed;
                    var percent = total > 0 ? downloaded * 100d / total : 0;
                    DownloadProgress.IsIndeterminate = total <= 0; DownloadProgress.Value = Math.Clamp(percent, 0, 100);
                    DownloadProgressText.Text = total > 0 ? $"{percent:0.0}% · {FormatBytes(downloaded)} de {FormatBytes(total)}" : $"{FormatBytes(downloaded)} baixados";
                    DownloadSpeedText.Text = $"{FormatBytes((long)speed)}/s · {FormatRemaining(total, downloaded, speed)}";
                    lastBytes = downloaded; lastMs = clock.ElapsedMilliseconds;
                }
            }
            await output.FlushAsync(downloadCancellation.Token); output.Close(); File.Move(partialTarget, target, true);
            DownloadProgress.Value = 100; DownloadProgressText.Text = "Download concluído"; DownloadSpeedText.Text = FormatBytes(new FileInfo(target).Length);
            Log("Download concluído: " + target);
            var index = models.IndexOf(m); if (index >= 0) models[index] = m with { LocalPath = target };
            RefreshModelLists(); ModelsBox.SelectedItem = models.First(x => string.Equals(x.File, m.File, StringComparison.OrdinalIgnoreCase));
        }
        catch (OperationCanceledException) { Log("Download pausado. Clique em Retomar para continuar depois."); DownloadProgressText.Text = "Download pausado"; }
        catch (Exception ex) { Log("Erro no download: " + ex.Message); DownloadProgressText.Text = "Falha no download"; }
        finally { downloadCancellation?.Dispose(); downloadCancellation = null; downloadingModel = null; DownloadButton.IsEnabled = true; ModelsBox_SelectionChanged(this, null!); }
    }
    void Start_Click(object sender, RoutedEventArgs e)
    {
        StartModel(Selected, true);
    }
    void StartModel(Model m, bool showErrors)
    {
        var file = ResolvedPath(m); if (!IsUsableFile(file)) { if (showErrors) System.Windows.MessageBox.Show("Baixe o modelo antes de iniciá-lo.", "AirCode"); return; }
        var llamaPath = FindLlamaServer(); if (llamaPath is null) { System.Windows.MessageBox.Show("O motor llama.cpp não foi encontrado. Reinstale o AirCode ou execute: winget install llama.cpp", "AirCode"); return; }
        var layers = GpuLayersBox.Text.Trim(); if (string.IsNullOrWhiteSpace(layers) || layers == "auto") layers = m.GpuLayers;
        var context = (ContextBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "4096";
        StopOwnedServer(); var startInfo = new ProcessStartInfo(llamaPath) { RedirectStandardError = true, RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
        foreach (var argument in new[] { "-m", file, "-ngl", layers, "-c", context, "--host", "127.0.0.1", "--port", "8080", "--flash-attn", "auto" }) startInfo.ArgumentList.Add(argument);
        try { server = new Process { StartInfo = startInfo }; server.Start(); runningModelPath = file; }
        catch (Exception ex) { server = null; runningModelPath = null; ChatModelBox.IsEnabled = true; StartButton.IsEnabled = true; StartButton.Content = "Iniciar"; StateText.Text = "Falha ao abrir o motor"; Log("Falha ao iniciar: " + ex.Message); if (showErrors) System.Windows.MessageBox.Show(ex.Message, "Falha ao iniciar modelo"); return; }
        File.WriteAllText(ServerPidFile, server.Id.ToString());
        _ = ReadServerLog(server.StandardOutput); _ = ReadServerLog(server.StandardError);
        StateDot.Fill = new Media.SolidColorBrush(Media.Color.FromRgb(245, 166, 35)); StateText.Text = "Carregando " + m.Name + "…";
        StartButton.IsEnabled = false; StartButton.Content = "Carregando…"; ChatModelBox.IsEnabled = false;
        Log("Inicializando servidor local em http://127.0.0.1:8080");
        _ = MonitorServerReady(m, server);
    }
    string? FindLlamaServer()
    {
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var candidates = new List<string>
        {
            Path.Combine(AppContext.BaseDirectory, "llama-server.exe"),
            Path.Combine(root, "runtime", "llama-server.exe")
        };
        var packages = Path.Combine(local, "Microsoft", "WinGet", "Packages");
        if (Directory.Exists(packages)) try { candidates.AddRange(Directory.EnumerateFiles(packages, "llama-server.exe", SearchOption.AllDirectories)); } catch { }
        var pathFolders = (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
        candidates.AddRange(pathFolders.Select(folder => Path.Combine(folder.Trim('"'), "llama-server.exe")));
        return candidates.FirstOrDefault(File.Exists);
    }
    void StopOwnedServer()
    {
        try { if (server is { HasExited: false }) server.Kill(true); } catch { }
        try
        {
            if (File.Exists(ServerPidFile) && int.TryParse(File.ReadAllText(ServerPidFile), out var pid))
            {
                using var previous = Process.GetProcessById(pid); if (!previous.HasExited && previous.ProcessName.Equals("llama-server", StringComparison.OrdinalIgnoreCase)) previous.Kill(true);
            }
        }
        catch { }
        try { File.Delete(ServerPidFile); } catch { }
        runningModelPath = null;
    }
    void SetDefaultModel_Click(object sender, RoutedEventArgs e)
    {
        if (ModelsBox.SelectedItem is not Model model || !IsInstalled(model)) return;
        saved.DefaultModelPath = ResolvedPath(model); SaveState(); RefreshModelLists(); DefaultModelBox.SelectedItem = models.FirstOrDefault(m => SamePath(ResolvedPath(m), saved.DefaultModelPath)); ModelsBox_SelectionChanged(this, null!);
        Log(model.Name + " definido como modelo padrão.");
    }
    void RemoveModel_Click(object sender, RoutedEventArgs e)
    {
        if (ModelsBox.SelectedItem is not Model model) return; var path = ResolvedPath(model); var target = File.Exists(path) ? path : path + ".part"; if (!File.Exists(target)) return;
        if (System.Windows.MessageBox.Show($"Remover este arquivo do computador?\n\n{target}", "Remover modelo", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        try { if (server is { HasExited: false } && SamePath(path, runningModelPath)) StopOwnedServer(); File.Delete(target); if (SamePath(path, saved.DefaultModelPath)) saved.DefaultModelPath = ""; Log("Arquivo removido: " + target); RefreshModelLists(); SaveState(); ModelsBox_SelectionChanged(this, null!); }
        catch (Exception ex) { System.Windows.MessageBox.Show("Não foi possível remover o modelo: " + ex.Message, "AirCode"); }
    }
    void DefaultModelBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (loadingSettings || refreshingModelSelectors || DefaultModelBox.SelectedItem is not Model model) return;
        saved.DefaultModelPath = ResolvedPath(model); SaveState(); ModelsBox_SelectionChanged(this, null!);
    }
    void ChatModelBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (loadingSettings || refreshingModelSelectors || ChatModelBox.SelectedItem is not Model model || SamePath(ResolvedPath(model), runningModelPath)) return;
        if (isSending) { System.Windows.MessageBox.Show("Pare a resposta atual antes de trocar o modelo.", "AirCode"); refreshingModelSelectors = true; ChatModelBox.SelectedItem = models.FirstOrDefault(m => SamePath(ResolvedPath(m), runningModelPath)); refreshingModelSelectors = false; return; }
        StartModel(model, true);
    }
    static string FormatBytes(long bytes) { string[] units = ["B", "KB", "MB", "GB", "TB"]; double value = bytes; var unit = 0; while (value >= 1024 && unit < units.Length - 1) { value /= 1024; unit++; } return $"{value:0.##} {units[unit]}"; }
    static string FormatRemaining(long total, long current, double speed) { if (total <= current || speed <= 0) return "calculando tempo"; var time = TimeSpan.FromSeconds((total - current) / speed); return time.TotalHours >= 1 ? $"{(int)time.TotalHours}h {time.Minutes}min restantes" : time.TotalMinutes >= 1 ? $"{(int)time.TotalMinutes}min restantes" : $"{Math.Max(1, time.Seconds)}s restantes"; }
    async Task ReadServerLog(StreamReader reader)
    {
        string? line;
        while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) is not null) Log(line);
    }
    async Task MonitorServerReady(Model model, Process process)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        for (var attempt = 0; attempt < 240 && !process.HasExited; attempt++)
        {
            try
            {
                using var response = await client.GetAsync("http://127.0.0.1:8080/health");
                if (response.IsSuccessStatusCode)
                {
                    await Dispatcher.InvokeAsync(() => { StateDot.Fill = new Media.SolidColorBrush(Media.Color.FromRgb(56, 211, 159)); StateText.Text = model.Name + " em execução"; StartButton.IsEnabled = true; StartButton.Content = "Reiniciar"; refreshingModelSelectors = true; ChatModelBox.SelectedItem = models.FirstOrDefault(m => SamePath(ResolvedPath(m), runningModelPath)); ChatModelBox.IsEnabled = true; refreshingModelSelectors = false; });
                    Log("Modelo pronto para conversar."); return;
                }
            }
            catch { }
            await Task.Delay(500);
        }
        await Dispatcher.InvokeAsync(() => { StartButton.IsEnabled = true; StartButton.Content = "Iniciar"; ChatModelBox.IsEnabled = true; StateDot.Fill = new Media.SolidColorBrush(Media.Color.FromRgb(224, 86, 86)); StateText.Text = process.HasExited ? "Falha ao iniciar o modelo" : "Inicialização demorando"; });
        if (process.HasExited) Log($"O servidor encerrou com código {process.ExitCode}.");
    }
    async void Send_Click(object sender, RoutedEventArgs e)
    {
        if (isSending) { generationCancellation?.Cancel(); return; }
        var prompt = PromptBox.Text.Trim(); if (string.IsNullOrEmpty(prompt)) return; PromptBox.Clear(); AddMessage("Você", prompt, true);
        if (TryExtractImagePrompt(prompt, out var imagePrompt)) { await GenerateChatImageAsync(prompt, imagePrompt); return; }
        if (server is null || server.HasExited) { AddMessage(ProductName, "Inicie um modelo na seção Modelos antes de conversar.", false); return; }
        isSending = true; generationCancellation = new CancellationTokenSource(); SendButton.Content = "■"; SendButton.ToolTip = "Parar geração";
        var activity = BeginActivity("Executando solicitação");
        try
        {
            AddActivityStep(activity, activeProject is null ? "Preparando conversa local" : $"Carregando contexto de {Path.GetFileName(activeProject)}");
            var useProjectTools = activeProject is not null && NeedsProjectTools(prompt);
            var requestMessages = BuildRecentHistory(useProjectTools);
            requestMessages.Add(new Dictionary<string, string> { ["role"] = "user", ["content"] = prompt });
            if (activeProject is not null) requestMessages.Insert(0, new Dictionary<string, string> { ["role"] = "system", ["content"] = useProjectTools ? BuildAgentContext() : $"Você é o AIR IA Code, assistente local criado por Codename Jackers para o projeto {Path.GetFileName(activeProject)}. Responda de forma curta e natural." });
            string? bootstrapName = null; string? bootstrapOutput = null;
            if (useProjectTools && GetBootstrapTool(prompt) is { } bootstrap)
            {
                UpdateActivity(activity, "Executando " + ToolDisplayName(bootstrap.Name) + "…");
                bootstrapName = bootstrap.Name; bootstrapOutput = await ExecuteProjectTool(bootstrap.Name, bootstrap.Arguments, activity);
                requestMessages.Add(new Dictionary<string, string> { ["role"] = "user", ["content"] = $"DADOS REAIS COLETADOS POR {bootstrap.Name}:\n{bootstrapOutput}\nResponda à solicitação original com conclusões concretas baseadas nesses dados." });
            }
            var temperature = double.TryParse(saved.Temperature, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsedTemperature) ? parsedTemperature : 0.35;
            AddActivityStep(activity, "Solicitação enviada ao modelo local"); UpdateActivity(activity, "Gerando resposta…");
            using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
            var answer = "";
            for (var round = 0; round < 12; round++)
            {
                var body = JsonSerializer.Serialize(new { messages = requestMessages, tools = useProjectTools ? BuildProjectTools() : null, tool_choice = useProjectTools ? "auto" : null, temperature, max_tokens = useProjectTools ? saved.MaxTokens : Math.Min(saved.MaxTokens, 320), stream = true });
                StreamingMessage? streamingMessage = null;
                var streamed = await StreamCompletion(client, body, token => Dispatcher.Invoke(() => { streamingMessage ??= BeginStreamingMessage(); streamingMessage.Buffer.Append(token); }), generationCancellation.Token);
                if (streamingMessage is not null) FinishStreamingMessage(streamingMessage);
                if (streamed.ToolCalls.Count > 0)
                {
                    if (streamingMessage is not null) Messages.Children.Remove(streamingMessage.Container);
                    requestMessages.Add(new Dictionary<string, object> { ["role"] = "assistant", ["content"] = streamed.Content, ["tool_calls"] = streamed.ToolCalls.Select(call => new { id = call.Id, type = "function", function = new { name = call.Name, arguments = call.Arguments } }).ToArray() });
                    foreach (var call in streamed.ToolCalls)
                    {
                        UpdateActivity(activity, "Executando " + ToolDisplayName(call.Name) + "…");
                        var toolResult = await ExecuteProjectTool(call.Name, call.Arguments, activity);
                        requestMessages.Add(new Dictionary<string, object> { ["role"] = "tool", ["tool_call_id"] = call.Id, ["content"] = toolResult });
                    }
                    UpdateActivity(activity, "Analisando resultado das ações…"); continue;
                }
                var responseContent = streamed.Content;
                if (TryParseTextToolCall(responseContent, out var textToolName, out var textToolArguments))
                {
                    if (streamingMessage is not null) Messages.Children.Remove(streamingMessage.Container);
                    requestMessages.Add(new Dictionary<string, string> { ["role"] = "assistant", ["content"] = responseContent });
                    UpdateActivity(activity, "Executando " + ToolDisplayName(textToolName) + "…");
                    var toolResult = await ExecuteProjectTool(textToolName, textToolArguments, activity);
                    requestMessages.Add(new Dictionary<string, string> { ["role"] = "user", ["content"] = $"Resultado da ferramenta {textToolName}:\n{toolResult}\nContinue a tarefa. Se terminou, responda normalmente sem outro JSON." });
                    UpdateActivity(activity, "Analisando resultado da ação…"); continue;
                }
                if (bootstrapOutput is not null)
                {
                    var verifiedResult = $"Resultado real · {ToolDisplayName(bootstrapName!)}\n\n{TrimText(bootstrapOutput, 12_000)}";
                    if (LooksLikeUnreliableActionAnswer(responseContent))
                    {
                        if (streamingMessage is not null) Messages.Children.Remove(streamingMessage.Container);
                        answer = verifiedResult; AddMessage("AirCode", answer, false);
                    }
                    else
                    {
                        answer = string.IsNullOrWhiteSpace(responseContent) ? verifiedResult : responseContent;
                        if (streamingMessage is null && !string.IsNullOrWhiteSpace(responseContent)) AddMessage("AirCode", responseContent, false);
                        AddMessage("AirCode", verifiedResult, false);
                        if (!string.IsNullOrWhiteSpace(responseContent)) answer += "\n\n" + verifiedResult;
                    }
                    break;
                }
                answer = string.IsNullOrWhiteSpace(responseContent) ? "Sem resposta." : responseContent;
                if (streamingMessage is null) AddMessage("AirCode", answer, false); break;
            }
            if (string.IsNullOrWhiteSpace(answer)) { answer = "Limite de ações atingido. Revise as etapas acima e peça para eu continuar."; AddMessage("AirCode", answer, false); }
            var codeBlocks = Math.Max(0, answer.Split("```", StringSplitOptions.None).Length / 2);
            AddActivityStep(activity, codeBlocks > 0 ? $"Resposta preparada com {codeBlocks} bloco(s) de código" : "Resposta preparada");
            history.Add(new() { ["role"] = "user", ["content"] = prompt }); history.Add(new() { ["role"] = "assistant", ["content"] = answer }); saved.Chats[ChatKey] = history.Select(x => new Dictionary<string, string>(x)).ToList();
            if (activeProject is not null) saved.ProjectMemories[ChatKey] = TrimText($"Última solicitação: {prompt}\nÚltima resposta: {answer}", 6000); SaveState();
            FinishActivity(activity, "Concluído");
        }
        catch (OperationCanceledException) { FinishActivity(activity, "Interrompido", true); }
        catch (Exception ex) { FinishActivity(activity, "Falha", true); AddMessage("AirCode", "Erro ao falar com o modelo: " + ex.Message, false); }
        finally { generationCancellation?.Dispose(); generationCancellation = null; isSending = false; SendButton.Content = "↑"; SendButton.ToolTip = "Enviar"; PromptBox.Focus(); }
    }
    sealed class StreamingMessage(Border container, TextBlock text, StringBuilder buffer, System.Windows.Threading.DispatcherTimer timer) { public Border Container { get; } = container; public TextBlock Text { get; } = text; public StringBuilder Buffer { get; } = buffer; public System.Windows.Threading.DispatcherTimer Timer { get; } = timer; }
    sealed record StreamedToolCall(string Id, string Name, string Arguments);
    sealed record StreamedResponse(string Content, List<StreamedToolCall> ToolCalls);
    sealed class ToolCallParts { public string Id { get; set; } = ""; public string Name { get; set; } = ""; public StringBuilder Arguments { get; } = new(); }
    StreamingMessage BeginStreamingMessage()
    {
        var text = new TextBlock { TextWrapping = TextWrapping.Wrap, Foreground = new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString("#ECEEEF")), FontSize = 14, LineHeight = 22 };
        var panel = new StackPanel { Children = { text } };
        var border = new Border { Background = Media.Brushes.Transparent, Padding = new Thickness(0, 10, 0, 14), Margin = new Thickness(0, 4, 70, 4), Child = panel };
        var buffer = new StringBuilder(); var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(32) }; timer.Tick += (_, _) => { var value = buffer.ToString(); if (text.Text != value) { text.Text = value; ChatScroll.ScrollToEnd(); } }; timer.Start();
        Messages.Children.Add(border); ChatScroll.ScrollToEnd(); return new StreamingMessage(border, text, buffer, timer);
    }
    void FinishStreamingMessage(StreamingMessage message) { message.Timer.Stop(); message.Text.Text = message.Buffer.ToString(); ChatScroll.ScrollToEnd(); }
    async Task<StreamedResponse> StreamCompletion(HttpClient client, string body, Action<string> onToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "http://127.0.0.1:8080/v1/chat/completions") { Content = new StringContent(body, Encoding.UTF8, "application/json") };
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken); response.EnsureSuccessStatusCode();
        if (!string.Equals(response.Content.Headers.ContentType?.MediaType, "text/event-stream", StringComparison.OrdinalIgnoreCase))
        {
            var json = await response.Content.ReadAsStringAsync(cancellationToken); using var doc = JsonDocument.Parse(json); var message = doc.RootElement.GetProperty("choices")[0].GetProperty("message"); var content = message.TryGetProperty("content", out var contentElement) ? contentElement.GetString() ?? "" : ""; if (content.Length > 0) onToken(content);
            var calls = new List<StreamedToolCall>(); if (message.TryGetProperty("tool_calls", out var toolCalls)) foreach (var call in toolCalls.EnumerateArray()) { var function = call.GetProperty("function"); calls.Add(new StreamedToolCall(call.GetProperty("id").GetString() ?? Guid.NewGuid().ToString(), function.GetProperty("name").GetString() ?? "", function.GetProperty("arguments").GetString() ?? "{}")); }
            return new StreamedResponse(content, calls);
        }
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken); using var reader = new StreamReader(stream); var contentBuilder = new StringBuilder(); var toolParts = new Dictionary<int, ToolCallParts>();
        string? line; while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
        {
            if (!line.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) continue; var data = line[5..].Trim(); if (data == "[DONE]") break; if (string.IsNullOrWhiteSpace(data)) continue;
            using var chunk = JsonDocument.Parse(data); var choices = chunk.RootElement.GetProperty("choices"); if (choices.GetArrayLength() == 0) continue; var delta = choices[0].GetProperty("delta");
            if (delta.TryGetProperty("content", out var tokenElement) && tokenElement.ValueKind == JsonValueKind.String) { var token = tokenElement.GetString() ?? ""; contentBuilder.Append(token); if (token.Length > 0) onToken(token); }
            if (delta.TryGetProperty("tool_calls", out var callsElement)) foreach (var call in callsElement.EnumerateArray())
            {
                var index = call.TryGetProperty("index", out var indexElement) ? indexElement.GetInt32() : 0; if (!toolParts.TryGetValue(index, out var parts)) toolParts[index] = parts = new();
                if (call.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.String) parts.Id += idElement.GetString();
                if (call.TryGetProperty("function", out var function)) { if (function.TryGetProperty("name", out var nameElement) && nameElement.ValueKind == JsonValueKind.String) parts.Name += nameElement.GetString(); if (function.TryGetProperty("arguments", out var argsElement) && argsElement.ValueKind == JsonValueKind.String) parts.Arguments.Append(argsElement.GetString()); }
            }
        }
        return new StreamedResponse(contentBuilder.ToString(), toolParts.OrderBy(pair => pair.Key).Select(pair => new StreamedToolCall(string.IsNullOrWhiteSpace(pair.Value.Id) ? Guid.NewGuid().ToString() : pair.Value.Id, pair.Value.Name, pair.Value.Arguments.Length == 0 ? "{}" : pair.Value.Arguments.ToString())).ToList());
    }
    sealed record ActivityCard(TextBlock Status, StackPanel Steps, TextBlock LiveOutput, StackPanel Details, TextBlock Chevron, Stopwatch Timer);
    ActivityCard BeginActivity(string title)
    {
        var status = new TextBlock { Text = title, FontSize = 12, Foreground = new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString("#8E969A")), VerticalAlignment = VerticalAlignment.Center };
        var chevron = new TextBlock { Text = "›", FontSize = 15, Foreground = new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString("#737B7F")), Margin = new Thickness(0, -2, 7, 0) };
        var header = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal, Children = { chevron, new System.Windows.Shapes.Ellipse { Width = 5, Height = 5, Fill = new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString("#8B5CF6")), Margin = new Thickness(0, 0, 9, 0) }, status } };
        var steps = new StackPanel { Margin = new Thickness(0, 3, 0, 5) };
        var liveOutput = new TextBlock { FontFamily = new Media.FontFamily("Cascadia Mono,Consolas"), FontSize = 11, Foreground = new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString("#929DA2")), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 6, 0, 0), MaxHeight = 260 };
        var content = new StackPanel { Margin = new Thickness(17, 6, 0, 4), Visibility = Visibility.Collapsed }; content.Children.Add(steps); content.Children.Add(new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = liveOutput });
        var toggle = new System.Windows.Controls.Button { Content = header, Background = Media.Brushes.Transparent, BorderThickness = new Thickness(0), Padding = new Thickness(0), HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left, Cursor = System.Windows.Input.Cursors.Hand };
        toggle.Click += (_, _) => { content.Visibility = content.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible; chevron.Text = content.Visibility == Visibility.Visible ? "⌄" : "›"; };
        var container = new StackPanel { Margin = new Thickness(0, 7, 80, 5), Children = { toggle, content } };
        Messages.Children.Add(container); ChatScroll.ScrollToEnd(); return new ActivityCard(status, steps, liveOutput, content, chevron, Stopwatch.StartNew());
    }
    void AddActivityStep(ActivityCard card, string text) { card.Steps.Children.Add(new TextBlock { Text = text, Foreground = new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString("#8F999E")), FontSize = 11, Margin = new Thickness(0, 2, 0, 2) }); ChatScroll.ScrollToEnd(); }
    void AppendActivityOutput(ActivityCard card, string text) { Dispatcher.Invoke(() => { card.Details.Visibility = Visibility.Visible; card.Chevron.Text = "⌄"; card.LiveOutput.Text = TrimText(card.LiveOutput.Text + text + "\n", 60_000); ChatScroll.ScrollToEnd(); }); }
    void UpdateActivity(ActivityCard card, string text) { card.Status.Text = text; }
    void FinishActivity(ActivityCard card, string text, bool failed = false) { card.Timer.Stop(); card.Details.Visibility = Visibility.Collapsed; card.Chevron.Text = "›"; card.Status.Text = failed ? $"Falhou após {card.Timer.Elapsed.TotalSeconds:0.0}s" : $"Trabalhou por {card.Timer.Elapsed.TotalSeconds:0.0}s"; card.Status.Foreground = new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString(failed ? "#D98B8B" : "#7F878B")); }
    void PromptBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != Key.Enter || Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) return;
        e.Handled = true; Send_Click(sender, e);
    }
    void PromptBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (PromptPlaceholder is null || SendButton is null) return; var hasText = !string.IsNullOrWhiteSpace(PromptBox.Text);
        PromptPlaceholder.Visibility = hasText ? Visibility.Collapsed : Visibility.Visible;
        if (isSending) return;
        SendButton.Background = new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString(hasText ? "#8B7CFF" : "#4A4F52"));
        SendButton.Foreground = new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString(hasText ? "#FFFFFF" : "#B9BEC1"));
        SendButton.Opacity = hasText ? 1 : 0.72;
    }
    void PromptBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => ComposerBorder.BorderBrush = new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString("#7567B7"));
    void PromptBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => ComposerBorder.BorderBrush = new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString("#3B3F42"));
    void ChatImageButton_Click(object sender, RoutedEventArgs e)
    {
        if (isSending) return;
        var current = PromptBox.Text.Trim();
        if (!TryExtractImagePrompt(current, out _)) PromptBox.Text = string.IsNullOrWhiteSpace(current) ? "/imagem " : "/imagem " + current;
        PromptBox.Focus(); PromptBox.CaretIndex = PromptBox.Text.Length;
    }
    static bool TryExtractImagePrompt(string input, out string imagePrompt)
    {
        imagePrompt = ""; if (string.IsNullOrWhiteSpace(input)) return false;
        string[] prefixes = ["/imagem", "/image", "gere uma imagem", "gerar uma imagem", "crie uma imagem", "criar uma imagem", "faça uma imagem", "faca uma imagem", "gere imagem", "crie imagem"];
        var prefix = prefixes.FirstOrDefault(value => input.StartsWith(value, StringComparison.OrdinalIgnoreCase));
        if (prefix is null) return false;
        imagePrompt = input[prefix.Length..].TrimStart(' ', ':', '-', ',');
        if (imagePrompt.StartsWith("de ", StringComparison.OrdinalIgnoreCase)) imagePrompt = imagePrompt[3..].TrimStart();
        if (string.IsNullOrWhiteSpace(imagePrompt)) imagePrompt = "uma ilustração digital detalhada e bonita";
        return true;
    }
    void AddContext_Click(object sender, RoutedEventArgs e)
    {
        if (activeProject is null) { System.Windows.MessageBox.Show("Selecione um projeto antes de adicionar arquivos ao contexto.", "AirCode"); return; }
        using var dialog = new Forms.OpenFileDialog { Title = "Adicionar arquivo do projeto ao contexto", InitialDirectory = activeProject, CheckFileExists = true, Multiselect = false, Filter = "Arquivos de código e texto|*.cs;*.xaml;*.js;*.ts;*.tsx;*.jsx;*.java;*.kt;*.kts;*.py;*.json;*.xml;*.yml;*.yaml;*.md;*.txt;*.gradle;*.properties;*.html;*.css;*.cpp;*.h|Todos os arquivos|*.*" };
        if (dialog.ShowDialog() != Forms.DialogResult.OK) return;
        var fullPath = Path.GetFullPath(dialog.FileName); var projectRoot = Path.GetFullPath(activeProject).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase)) { System.Windows.MessageBox.Show("Escolha um arquivo que esteja dentro do projeto ativo.", "AirCode"); return; }
        var relative = Path.GetRelativePath(activeProject, fullPath).Replace('\\', '/'); var prefix = $"Analise o arquivo @{relative}\n\n";
        if (!PromptBox.Text.Contains("@" + relative, StringComparison.OrdinalIgnoreCase)) PromptBox.Text = prefix + PromptBox.Text;
        PromptBox.CaretIndex = PromptBox.Text.Length; PromptBox.Focus();
    }
    static object[] BuildProjectTools() =>
    [
        new { type = "function", function = new { name = "list_files", description = "Lista arquivos e pastas dentro do projeto.", parameters = new { type = "object", properties = new { path = new { type = "string", description = "Caminho relativo; use . para a raiz" } } } } },
        new { type = "function", function = new { name = "read_file", description = "Lê arquivo ou intervalo de linhas.", parameters = new { type = "object", properties = new { path = new { type = "string" }, start_line = new { type = "integer" }, end_line = new { type = "integer" } }, required = new[] { "path" } } } },
        new { type = "function", function = new { name = "search_text", description = "Pesquisa texto ou regex nos arquivos do projeto.", parameters = new { type = "object", properties = new { query = new { type = "string" }, path = new { type = "string" }, file_pattern = new { type = "string" } }, required = new[] { "query" } } } },
        new { type = "function", function = new { name = "write_file", description = "Cria ou substitui um arquivo no projeto com o conteúdo informado.", parameters = new { type = "object", properties = new { path = new { type = "string" }, content = new { type = "string" } }, required = new[] { "path", "content" } } } },
        new { type = "function", function = new { name = "edit_file", description = "Substitui exatamente um trecho existente sem reescrever o arquivo inteiro.", parameters = new { type = "object", properties = new { path = new { type = "string" }, old_text = new { type = "string" }, new_text = new { type = "string" } }, required = new[] { "path", "old_text", "new_text" } } } },
        new { type = "function", function = new { name = "create_directory", description = "Cria uma pasta no projeto.", parameters = new { type = "object", properties = new { path = new { type = "string" } }, required = new[] { "path" } } } },
        new { type = "function", function = new { name = "move_path", description = "Move ou renomeia arquivo/pasta.", parameters = new { type = "object", properties = new { source = new { type = "string" }, destination = new { type = "string" } }, required = new[] { "source", "destination" } } } },
        new { type = "function", function = new { name = "copy_file", description = "Copia um arquivo dentro do projeto.", parameters = new { type = "object", properties = new { source = new { type = "string" }, destination = new { type = "string" } }, required = new[] { "source", "destination" } } } },
        new { type = "function", function = new { name = "delete_path", description = "Exclui arquivo ou pasta do projeto. Exige confirmação do usuário.", parameters = new { type = "object", properties = new { path = new { type = "string" } }, required = new[] { "path" } } } },
        new { type = "function", function = new { name = "run_command", description = "Executa comando PowerShell e aguarda a saída; use para build, testes e diagnósticos.", parameters = new { type = "object", properties = new { command = new { type = "string" }, timeout_seconds = new { type = "integer" } }, required = new[] { "command" } } } },
        new { type = "function", function = new { name = "start_process", description = "Inicia servidor, aplicativo ou depurador em segundo plano e retorna PID.", parameters = new { type = "object", properties = new { command = new { type = "string" } }, required = new[] { "command" } } } },
        new { type = "function", function = new { name = "read_process_output", description = "Lê logs atuais de um processo iniciado pela IA.", parameters = new { type = "object", properties = new { pid = new { type = "integer" } }, required = new[] { "pid" } } } },
        new { type = "function", function = new { name = "stop_process", description = "Encerra um processo iniciado pela IA.", parameters = new { type = "object", properties = new { pid = new { type = "integer" } }, required = new[] { "pid" } } } },
        new { type = "function", function = new { name = "inspect_environment", description = "Obtém estrutura, tecnologia, Git, SDKs e processos do ambiente de desenvolvimento.", parameters = new { type = "object", properties = new { } } } },
        new { type = "function", function = new { name = "git_status", description = "Mostra branch e alterações do Git.", parameters = new { type = "object", properties = new { } } } },
        new { type = "function", function = new { name = "git_diff", description = "Mostra o diff Git atual.", parameters = new { type = "object", properties = new { staged = new { type = "boolean" } } } } },
        new { type = "function", function = new { name = "build_project", description = "Detecta a tecnologia e compila o projeto.", parameters = new { type = "object", properties = new { } } } },
        new { type = "function", function = new { name = "test_project", description = "Detecta e executa os testes do projeto.", parameters = new { type = "object", properties = new { } } } },
        new { type = "function", function = new { name = "web_search", description = "Pesquisa na internet por documentação, erros, pacotes e soluções atuais.", parameters = new { type = "object", properties = new { query = new { type = "string" } }, required = new[] { "query" } } } },
        new { type = "function", function = new { name = "fetch_url", description = "Abre uma página HTTP/HTTPS e extrai seu conteúdo textual.", parameters = new { type = "object", properties = new { url = new { type = "string" } }, required = new[] { "url" } } } },
        new { type = "function", function = new { name = "install_dependency", description = "Instala recurso ausente usando winget, npm, pip ou dotnet.", parameters = new { type = "object", properties = new { manager = new { type = "string", description = "winget, npm, pip ou dotnet" }, package = new { type = "string" } }, required = new[] { "manager", "package" } } } },
        new { type = "function", function = new { name = "android_devices", description = "Lista celulares e emuladores Android conectados via ADB.", parameters = new { type = "object", properties = new { } } } },
        new { type = "function", function = new { name = "android_logcat", description = "Lê ou acompanha Logcat em tempo real.", parameters = new { type = "object", properties = new { lines = new { type = "integer" }, filter = new { type = "string" }, follow = new { type = "boolean" } } } } },
        new { type = "function", function = new { name = "android_clear_logcat", description = "Limpa o buffer do Logcat.", parameters = new { type = "object", properties = new { } } } },
        new { type = "function", function = new { name = "android_install_apk", description = "Instala ou atualiza um APK no dispositivo conectado.", parameters = new { type = "object", properties = new { path = new { type = "string" } }, required = new[] { "path" } } } }
    ];
    static string ToolDisplayName(string name) => name switch { "list_files" => "leitura da pasta", "read_file" => "leitura de arquivo", "search_text" => "pesquisa no código", "write_file" or "edit_file" => "alteração de arquivo", "create_directory" => "criação de pasta", "move_path" => "movimentação", "copy_file" => "cópia", "delete_path" => "exclusão", "run_command" => "comando", "start_process" => "inicialização de processo", "read_process_output" => "leitura de logs", "stop_process" => "encerramento de processo", "inspect_environment" => "inspeção do ambiente", "git_status" or "git_diff" => "inspeção do Git", "build_project" => "compilação", "test_project" => "testes", "web_search" => "pesquisa na internet", "fetch_url" => "leitura da internet", "install_dependency" => "instalação de recurso", "android_devices" => "dispositivos Android", "android_logcat" => "Logcat", "android_clear_logcat" => "limpeza do Logcat", "android_install_apk" => "instalação do APK", _ => name };
    static bool TryParseTextToolCall(string content, out string name, out string arguments)
    {
        name = ""; arguments = "{}"; var start = content.IndexOf('{'); if (start < 0) return false; var depth = 0; var quoted = false; var escaped = false; var end = -1;
        for (var index = start; index < content.Length; index++)
        {
            var character = content[index]; if (quoted) { if (escaped) escaped = false; else if (character == '\\') escaped = true; else if (character == '"') quoted = false; continue; }
            if (character == '"') quoted = true; else if (character == '{') depth++; else if (character == '}' && --depth == 0) { end = index; break; }
        }
        if (end <= start) return false;
        try
        {
            using var json = JsonDocument.Parse(content[start..(end + 1)]); var root = json.RootElement;
            if (!root.TryGetProperty("name", out var nameElement)) return false; name = nameElement.GetString() ?? "";
            if (!new[] { "list_files", "read_file", "search_text", "write_file", "edit_file", "create_directory", "move_path", "copy_file", "delete_path", "run_command", "start_process", "read_process_output", "stop_process", "inspect_environment", "git_status", "git_diff", "build_project", "test_project", "web_search", "fetch_url", "install_dependency", "android_devices", "android_logcat", "android_clear_logcat", "android_install_apk" }.Contains(name)) return false;
            if (root.TryGetProperty("arguments", out var argsElement)) arguments = argsElement.ValueKind == JsonValueKind.String ? argsElement.GetString() ?? "{}" : argsElement.GetRawText();
            return true;
        }
        catch { return false; }
    }
    string ResolveProjectPath(string relative)
    {
        if (activeProject is null) throw new InvalidOperationException("Nenhum projeto selecionado.");
        if (relative is "/" or "\\") relative = ".";
        var rootPath = Path.GetFullPath(activeProject).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var resolved = Path.GetFullPath(Path.Combine(activeProject, string.IsNullOrWhiteSpace(relative) ? "." : relative));
        if (!resolved.Equals(rootPath.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase) && !resolved.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase)) throw new UnauthorizedAccessException("A ação tentou acessar um caminho fora do projeto selecionado.");
        return resolved;
    }
    List<object> BuildRecentHistory(bool programmingTask)
    {
        var recent = new List<object>(); var totalCharacters = 0; var messageLimit = programmingTask ? 12 : 6; var characterLimit = programmingTask ? 12_000 : 3_000;
        for (var index = history.Count - 1; index >= 0 && recent.Count < messageLimit; index--)
        {
            var message = history[index]; var content = TrimText(message.GetValueOrDefault("content", ""), programmingTask ? 4000 : 1000); if (totalCharacters + content.Length > characterLimit) break;
            recent.Add(new Dictionary<string, string> { ["role"] = message.GetValueOrDefault("role", "user"), ["content"] = content }); totalCharacters += content.Length;
        }
        recent.Reverse(); return recent;
    }
    static bool NeedsProjectTools(string prompt)
    {
        if (prompt.Length > 90) return true; var normalized = prompt.ToLowerInvariant();
        string[] programmingWords = ["arquivo", "pasta", "projeto", "código", "codigo", "erro", "bug", "corrija", "faça", "faca", "crie", "altere", "edite", "compile", "build", "teste", "execute", "rode", "app", "android", "adb", "logcat", "função", "funcao", "classe", "tela", "analise", "verifique", "olhe", "procure", "instale", "comando", "terminal", "git", "depur", "estrutura", "dependência", "dependencia", "continue", "continua", "implemente"];
        return programmingWords.Any(normalized.Contains);
    }
    sealed record BootstrapTool(string Name, string Arguments);
    static BootstrapTool? GetBootstrapTool(string prompt)
    {
        var value = prompt.ToLowerInvariant();
        if (value.Contains("logcat")) return new("android_logcat", JsonSerializer.Serialize(new { lines = 1200, filter = "", follow = false }));
        if (value.Contains("adb") || value.Contains("aparelho") || value.Contains("dispositivo")) return new("android_devices", "{}");
        if (value.Contains("compile") || value.Contains("build")) return new("build_project", "{}");
        if (value.Contains("teste") || value.Contains("testes")) return new("test_project", "{}");
        if (value.Contains("git status") || value.Contains("status do git")) return new("git_status", "{}");
        if ((value.Contains("analise") || value.Contains("verifique") || value.Contains("olhe")) && value.Contains("projeto")) return new("list_files", JsonSerializer.Serialize(new { path = "." }));
        if (value.Contains("ambiente") || value.Contains("sdk") || value.Contains("dependência") || value.Contains("dependencia")) return new("inspect_environment", "{}");
        return null;
    }
    static bool LooksLikeUnreliableActionAnswer(string answer)
    {
        if (string.IsNullOrWhiteSpace(answer)) return true; var value = answer.ToLowerInvariant(); string[] markers = ["vou executar", "posso continuar", "você pode usar", "voce pode usar", "não tenho acesso", "nao tenho acesso", "forneça o caminho", "forneca o caminho", "não diga que vai executar", "ação inicial obrigatória", "continue a solicitação original"]; return markers.Any(value.Contains);
    }
    string BuildAgentContext()
    {
        var memory = saved.ProjectMemories.TryGetValue(ChatKey, out var remembered) ? remembered : "Nenhuma tarefa anterior registrada.";
        var actions = saved.ProjectActions.TryGetValue(ChatKey, out var log) ? string.Join("\n", log.TakeLast(20)) : "Nenhuma ação anterior registrada.";
        return $$$"""
Você é o agente principal de programação do AIR IA Code, desenvolvido por Codename Jackers. Tem acesso completo e exclusivo à pasta: {{{activeProject}}}
Observe o ambiente antes de agir, use ferramentas para obter fatos e execute a tarefa até validar o resultado.
Você pode pesquisar e ler código, editar trechos, criar/mover/copiar/excluir itens, usar Git, compilar, testar, iniciar aplicações e analisar saídas.
Se uma ação falhar por falta de conhecimento, ferramenta, SDK ou dependência, pesquise na internet, consulte a documentação, instale o recurso necessário, tente novamente e valide o resultado. Não pare apenas no primeiro erro.
Para modelos sem tool_calls nativo, solicite uma ferramenta por vez usando somente JSON: {"name":"nome","arguments":{...}}.
Nunca afirme que alterou, compilou ou testou algo sem executar a ferramenta correspondente. Não acesse caminhos fora do projeto.

AMBIENTE ATUAL
{{{GetProjectSummary()}}}

MEMÓRIA DA ÚLTIMA SESSÃO
{{{memory}}}

AÇÕES RECENTES
{{{actions}}}
""";
    }
    string GetProjectSummary()
    {
        if (activeProject is null) return "Nenhum projeto selecionado.";
        var manifests = new[] { "*.sln", "*.csproj", "package.json", "pyproject.toml", "requirements.txt", "pom.xml", "build.gradle", "build.gradle.kts", "settings.gradle", "pubspec.yaml", "Cargo.toml", "go.mod", "CMakeLists.txt", "Dockerfile" };
        var detected = new List<string>();
        foreach (var pattern in manifests) detected.AddRange(EnumerateProjectFiles(activeProject, pattern, 20).Select(path => Path.GetRelativePath(activeProject, path)));
        var branch = "sem Git"; try { var head = Path.Combine(activeProject, ".git", "HEAD"); if (File.Exists(head)) branch = File.ReadAllText(head).Trim().Replace("ref: refs/heads/", "branch "); } catch { }
        return $"Windows {Environment.OSVersion.Version} · {Environment.ProcessorCount} processadores · .NET {Environment.Version}\nGit: {branch}\nManifestos: {(detected.Count == 0 ? "nenhum detectado" : string.Join(", ", detected.Take(30)))}\nProcessos da IA ativos: {(agentProcesses.Count == 0 ? "nenhum" : string.Join(", ", agentProcesses.Keys))}";
    }
    void RecordProjectAction(string action)
    {
        if (activeProject is null) return; if (!saved.ProjectActions.TryGetValue(ChatKey, out var log)) saved.ProjectActions[ChatKey] = log = new();
        log.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {action}"); if (log.Count > 200) log.RemoveRange(0, log.Count - 200); SaveState();
    }
    static string TrimText(string text, int max) => text.Length <= max ? text : text[..max] + "\n…[saída reduzida]";
    static IEnumerable<string> EnumerateProjectFiles(string rootPath, string pattern = "*", int max = 5000)
    {
        var ignored = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".git", ".idea", ".gradle", ".kotlin", "node_modules", "bin", "obj", "build", "dist", "out", "release", ".next", ".cache" };
        var pending = new Queue<string>(); pending.Enqueue(rootPath); var count = 0;
        while (pending.Count > 0 && count < max)
        {
            var directory = pending.Dequeue(); string[] files; string[] directories;
            try { files = Directory.GetFiles(directory, pattern, SearchOption.TopDirectoryOnly); directories = Directory.GetDirectories(directory); } catch { continue; }
            foreach (var file in files) { yield return file; if (++count >= max) yield break; }
            foreach (var child in directories) if (!ignored.Contains(Path.GetFileName(child))) pending.Enqueue(child);
        }
    }
    string DetectProjectCommand(bool tests)
    {
        if (activeProject is null) return "";
        if (Directory.EnumerateFiles(activeProject, "*.sln", SearchOption.TopDirectoryOnly).Any() || EnumerateProjectFiles(activeProject, "*.csproj", 1).Any()) return tests ? "dotnet test" : "dotnet build";
        if (File.Exists(Path.Combine(activeProject, "package.json"))) return tests ? "npm test -- --run" : "npm run build";
        if (File.Exists(Path.Combine(activeProject, "gradlew.bat"))) return tests ? ".\\gradlew.bat test" : ".\\gradlew.bat build";
        if (File.Exists(Path.Combine(activeProject, "pyproject.toml")) || File.Exists(Path.Combine(activeProject, "requirements.txt"))) return tests ? "python -m pytest" : "python -m compileall .";
        if (File.Exists(Path.Combine(activeProject, "Cargo.toml"))) return tests ? "cargo test" : "cargo build";
        if (File.Exists(Path.Combine(activeProject, "go.mod"))) return tests ? "go test ./..." : "go build ./...";
        return tests ? "Write-Output 'Nenhum executor de testes detectado.'" : "Get-ChildItem";
    }
    async Task<(int ExitCode, string Output)> RunAgentCommand(string command, int timeoutSeconds, Action<string>? onOutput = null)
    {
        using var process = new Process { StartInfo = CreatePowerShellStartInfo(command, activeProject!) };
        process.Start(); var output = new StringBuilder();
        async Task ReadLive(StreamReader reader, string prefix) { string? line; while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) is not null) { lock (output) output.AppendLine(prefix + line); onOutput?.Invoke(prefix + line); } }
        var outputTask = ReadLive(process.StandardOutput, ""); var errorTask = ReadLive(process.StandardError, "erro> ");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(Math.Clamp(timeoutSeconds, 5, 1800)));
        try { await process.WaitForExitAsync(timeout.Token); } catch (OperationCanceledException) { try { process.Kill(true); } catch { } return (-1, "Comando encerrado por tempo limite."); }
        await Task.WhenAll(outputTask, errorTask); return (process.ExitCode, TrimText(output.ToString(), 120_000));
    }
    static ProcessStartInfo CreatePowerShellStartInfo(string command, string workingDirectory)
    {
        var info = new ProcessStartInfo("powershell.exe") { WorkingDirectory = workingDirectory, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
        info.ArgumentList.Add("-NoLogo"); info.ArgumentList.Add("-NoProfile"); info.ArgumentList.Add("-NonInteractive"); info.ArgumentList.Add("-Command"); info.ArgumentList.Add(command); return info;
    }
    static string? FindAdbPath()
    {
        var candidates = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "WinGet", "Links", "adb.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Android", "Sdk", "platform-tools", "adb.exe"),
            Path.Combine(Environment.GetEnvironmentVariable("ANDROID_HOME") ?? "", "platform-tools", "adb.exe"),
            Path.Combine(Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT") ?? "", "platform-tools", "adb.exe")
        };
        return candidates.FirstOrDefault(File.Exists);
    }
    static async Task<Uri> ValidateInternetUri(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https")) throw new InvalidOperationException("URL HTTP/HTTPS inválida.");
        var addresses = await System.Net.Dns.GetHostAddressesAsync(uri.Host); foreach (var address in addresses)
        {
            if (System.Net.IPAddress.IsLoopback(address)) throw new UnauthorizedAccessException("Endereços locais não podem ser acessados pela ferramenta de internet.");
            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) { var bytes = address.GetAddressBytes(); if (bytes[0] == 10 || bytes[0] == 127 || (bytes[0] == 169 && bytes[1] == 254) || (bytes[0] == 172 && bytes[1] is >= 16 and <= 31) || (bytes[0] == 192 && bytes[1] == 168)) throw new UnauthorizedAccessException("Endereços privados não podem ser acessados pela ferramenta de internet."); }
            else if (address.IsIPv6LinkLocal || address.IsIPv6SiteLocal) throw new UnauthorizedAccessException("Endereços privados não podem ser acessados pela ferramenta de internet.");
        }
        return uri;
    }
    static async Task<string> DownloadWebText(string url)
    {
        var uri = await ValidateInternetUri(url); using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) }; client.DefaultRequestHeaders.UserAgent.ParseAdd("AirCodeLocal/0.4");
        var text = await client.GetStringAsync(uri); text = System.Text.RegularExpressions.Regex.Replace(text, "<script[\\s\\S]*?</script>|<style[\\s\\S]*?</style>", " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase); text = System.Text.RegularExpressions.Regex.Replace(text, "<[^>]+>", " "); text = System.Net.WebUtility.HtmlDecode(text); text = System.Text.RegularExpressions.Regex.Replace(text, "[ \\t]+", " "); text = System.Text.RegularExpressions.Regex.Replace(text, "(\\r?\\n){3,}", "\n\n"); return TrimText(text.Trim(), 50_000);
    }
    async Task<string> ExecuteProjectTool(string name, string arguments, ActivityCard activity)
    {
        using var args = JsonDocument.Parse(arguments); var rootArgs = args.RootElement;
        string Arg(string key, string fallback = "") => rootArgs.TryGetProperty(key, out var value) ? value.GetString() ?? fallback : fallback;
        int IntArg(string key, int fallback) => rootArgs.TryGetProperty(key, out var value) && value.TryGetInt32(out var number) ? number : fallback;
        bool BoolArg(string key) => rootArgs.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.True;
        try
        {
            if (name == "list_files")
            {
                var path = ResolveProjectPath(Arg("path", ".")); if (!Directory.Exists(path)) return "Pasta não encontrada.";
                var entries = Directory.EnumerateFileSystemEntries(path).Take(400).Select(item => (Directory.Exists(item) ? "[pasta] " : "[arquivo] ") + Path.GetRelativePath(activeProject!, item));
                AddActivityStep(activity, "Pasta consultada: " + Path.GetRelativePath(activeProject!, path)); return string.Join("\n", entries);
            }
            if (name == "read_file")
            {
                var path = ResolveProjectPath(Arg("path")); if (!File.Exists(path)) return "Arquivo não encontrado."; var info = new FileInfo(path); if (info.Length > 2_000_000) return "Arquivo maior que 2 MB; leia uma versão reduzida.";
                var lines = await File.ReadAllLinesAsync(path); var start = Math.Clamp(IntArg("start_line", 1), 1, Math.Max(1, lines.Length)); var end = Math.Clamp(IntArg("end_line", lines.Length), start, lines.Length);
                var content = string.Join("\n", lines.Skip(start - 1).Take(end - start + 1).Select((line, index) => $"{start + index}: {line}")); AddActivityStep(activity, $"Arquivo lido: {Path.GetRelativePath(activeProject!, path)} (linhas {start}-{end})"); return content;
            }
            if (name == "search_text")
            {
                var path = ResolveProjectPath(Arg("path", ".")); var query = Arg("query"); var pattern = Arg("file_pattern", "*"); var regex = new System.Text.RegularExpressions.Regex(query, System.Text.RegularExpressions.RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2)); var results = new List<string>();
                foreach (var file in EnumerateProjectFiles(path, pattern, 3000))
                {
                    try { var lineNumber = 0; foreach (var line in File.ReadLines(file)) { lineNumber++; if (regex.IsMatch(line)) { results.Add($"{Path.GetRelativePath(activeProject!, file)}:{lineNumber}: {TrimText(line.Trim(), 300)}"); if (results.Count >= 300) break; } } } catch { } if (results.Count >= 300) break;
                }
                AddActivityStep(activity, $"Pesquisa no código: {query} ({results.Count} resultado(s))"); return results.Count == 0 ? "Nenhum resultado." : string.Join("\n", results);
            }
            if (name == "write_file")
            {
                var path = ResolveProjectPath(Arg("path")); var content = Arg("content"); var existed = File.Exists(path); Directory.CreateDirectory(Path.GetDirectoryName(path)!); await File.WriteAllTextAsync(path, content); BuildTree(activeProject!);
                var action = (existed ? "Arquivo alterado: " : "Arquivo criado: ") + Path.GetRelativePath(activeProject!, path); AddActivityStep(activity, action); RecordProjectAction(action); return "Sucesso. " + action;
            }
            if (name == "edit_file")
            {
                var path = ResolveProjectPath(Arg("path")); if (!File.Exists(path)) return "Arquivo não encontrado."; var oldText = Arg("old_text"); var newText = Arg("new_text"); var content = await File.ReadAllTextAsync(path); var index = content.IndexOf(oldText, StringComparison.Ordinal); if (index < 0) return "Trecho old_text não encontrado; leia o arquivo novamente.";
                content = content.Remove(index, oldText.Length).Insert(index, newText); await File.WriteAllTextAsync(path, content); var action = "Trecho editado: " + Path.GetRelativePath(activeProject!, path); AddActivityStep(activity, action); RecordProjectAction(action); return action;
            }
            if (name == "create_directory")
            {
                var path = ResolveProjectPath(Arg("path")); Directory.CreateDirectory(path); BuildTree(activeProject!); var action = "Pasta criada: " + Path.GetRelativePath(activeProject!, path); AddActivityStep(activity, action); RecordProjectAction(action); return action;
            }
            if (name is "move_path" or "copy_file")
            {
                var source = ResolveProjectPath(Arg("source")); var destination = ResolveProjectPath(Arg("destination")); Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                if (name == "copy_file") File.Copy(source, destination, false); else if (File.Exists(source)) File.Move(source, destination); else if (Directory.Exists(source)) Directory.Move(source, destination); else return "Origem não encontrada.";
                BuildTree(activeProject!); var action = (name == "copy_file" ? "Arquivo copiado: " : "Item movido: ") + $"{Path.GetRelativePath(activeProject!, source)} → {Path.GetRelativePath(activeProject!, destination)}"; AddActivityStep(activity, action); RecordProjectAction(action); return action;
            }
            if (name == "delete_path")
            {
                var path = ResolveProjectPath(Arg("path")); if (path.Equals(Path.GetFullPath(activeProject!), StringComparison.OrdinalIgnoreCase)) return "A raiz do projeto não pode ser excluída.";
                var relative = Path.GetRelativePath(activeProject!, path); if (saved.ConfirmCommands && System.Windows.MessageBox.Show($"A IA quer excluir:\n{relative}\n\nContinuar?", "Confirmar exclusão", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return "Exclusão recusada pelo usuário.";
                if (File.Exists(path)) File.Delete(path); else if (Directory.Exists(path)) Directory.Delete(path, true); else return "Caminho não encontrado."; BuildTree(activeProject!); AddActivityStep(activity, "Item excluído: " + relative); RecordProjectAction("Item excluído: " + relative); return "Excluído: " + relative;
            }
            if (name == "run_command")
            {
                var command = Arg("command"); if (saved.ConfirmCommands && System.Windows.MessageBox.Show($"A IA quer executar no projeto:\n\n{command}", "Confirmar comando", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return "Comando recusado pelo usuário.";
                AddActivityStep(activity, "Executando comando"); AppendActivityOutput(activity, "> " + command); var result = await RunAgentCommand(command, IntArg("timeout_seconds", 300), line => AppendActivityOutput(activity, line)); var action = $"Comando executado (código {result.ExitCode}): {command}"; AddActivityStep(activity, action); RecordProjectAction(action); TerminalOutput.Text += $"> {command}\n{result.Output}\n[código {result.ExitCode}]\n"; return result.Output + $"\nCódigo de saída: {result.ExitCode}";
            }
            if (name == "start_process")
            {
                var command = Arg("command"); if (saved.ConfirmCommands && System.Windows.MessageBox.Show($"A IA quer iniciar um processo no projeto:\n\n{command}", "Confirmar execução", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return "Processo recusado pelo usuário.";
                AppendActivityOutput(activity, "> iniciar: " + command); var process = new Process { StartInfo = CreatePowerShellStartInfo(command, activeProject!), EnableRaisingEvents = true }; process.Start(); agentProcesses[process.Id] = process; agentProcessLogs[process.Id] = new StringBuilder();
                void Capture(string? line) { if (line is null) return; lock (agentProcessLogs) { if (agentProcessLogs.TryGetValue(process.Id, out var log)) { log.AppendLine(line); if (log.Length > 200_000) log.Remove(0, log.Length - 150_000); } } AppendActivityOutput(activity, $"[{process.Id}] {line}"); Dispatcher.Invoke(() => TerminalOutput.Text += $"[{process.Id}] {line}\n"); }
                process.OutputDataReceived += (_, e) => Capture(e.Data); process.ErrorDataReceived += (_, e) => Capture(e.Data); process.Exited += (_, _) => Dispatcher.Invoke(() => { Capture("processo encerrado"); agentProcesses.Remove(process.Id); }); process.BeginOutputReadLine(); process.BeginErrorReadLine(); var action = $"Processo iniciado PID {process.Id}: {command}"; AddActivityStep(activity, action); RecordProjectAction(action); return action;
            }
            if (name == "read_process_output")
            {
                var pid = IntArg("pid", 0); lock (agentProcessLogs) { if (!agentProcessLogs.TryGetValue(pid, out var log)) return "PID sem logs registrados."; AddActivityStep(activity, "Logs lidos do PID " + pid); return TrimText(log.ToString(), 120_000); }
            }
            if (name == "stop_process")
            {
                var pid = IntArg("pid", 0); if (!agentProcesses.TryGetValue(pid, out var process)) return "PID não pertence a um processo iniciado pela IA."; if (!process.HasExited) process.Kill(true); agentProcesses.Remove(pid); var action = "Processo encerrado PID " + pid; AddActivityStep(activity, action); RecordProjectAction(action); return action;
            }
            if (name == "inspect_environment")
            {
                var command = "$PSVersionTable.PSVersion.ToString(); dotnet --list-sdks; node --version; npm --version; python --version; git --version"; AppendActivityOutput(activity, "> " + command); var result = await RunAgentCommand(command, 30, line => AppendActivityOutput(activity, line)); AddActivityStep(activity, "Ambiente e SDKs inspecionados"); return GetProjectSummary() + "\n\nSDKs:\n" + result.Output;
            }
            if (name is "git_status" or "git_diff")
            {
                var command = name == "git_status" ? "git status --short --branch" : (BoolArg("staged") ? "git diff --cached" : "git diff"); AppendActivityOutput(activity, "> " + command); var result = await RunAgentCommand(command, 60, line => AppendActivityOutput(activity, line)); AddActivityStep(activity, name == "git_status" ? "Status do Git consultado" : "Diff do Git consultado"); return result.Output;
            }
            if (name is "build_project" or "test_project")
            {
                var command = DetectProjectCommand(name == "test_project"); UpdateActivity(activity, name == "test_project" ? "Executando testes…" : "Compilando projeto…"); AppendActivityOutput(activity, "> " + command); var result = await RunAgentCommand(command, 900, line => AppendActivityOutput(activity, line)); var action = $"{(name == "test_project" ? "Testes" : "Build")} concluído com código {result.ExitCode}"; AddActivityStep(activity, action); RecordProjectAction(action); TerminalOutput.Text += $"> {command}\n{result.Output}\n[código {result.ExitCode}]\n"; return result.Output + $"\nCódigo de saída: {result.ExitCode}";
            }
            if (name == "web_search")
            {
                var query = Arg("query"); AddActivityStep(activity, "Pesquisando na internet: " + query); AppendActivityOutput(activity, "> pesquisar: " + query); using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) }; client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 AirCodeLocal/0.4"); var html = await client.GetStringAsync("https://html.duckduckgo.com/html/?q=" + Uri.EscapeDataString(query));
                var matches = System.Text.RegularExpressions.Regex.Matches(html, "class=\"result__a\"[^>]*href=\"(?<url>[^\"]+)\"[^>]*>(?<title>[\\s\\S]*?)</a>", System.Text.RegularExpressions.RegexOptions.IgnoreCase); var results = new List<string>();
                foreach (System.Text.RegularExpressions.Match match in matches.Take(10)) { var url = System.Net.WebUtility.HtmlDecode(match.Groups["url"].Value); var redirect = System.Text.RegularExpressions.Regex.Match(url, "[?&]uddg=([^&]+)"); if (redirect.Success) url = Uri.UnescapeDataString(redirect.Groups[1].Value); if (url.StartsWith("//")) url = "https:" + url; var title = System.Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Replace(match.Groups["title"].Value, "<[^>]+>", "")); results.Add($"{title}\n{url}"); }
                AddActivityStep(activity, $"Pesquisa concluída · {results.Count} resultado(s)"); return results.Count == 0 ? "Nenhum resultado encontrado." : string.Join("\n\n", results);
            }
            if (name == "fetch_url")
            {
                var url = Arg("url"); AddActivityStep(activity, "Abrindo documentação: " + url); AppendActivityOutput(activity, "> abrir: " + url); var content = await DownloadWebText(url); AddActivityStep(activity, $"Página lida · {content.Length} caracteres"); return content;
            }
            if (name == "install_dependency")
            {
                var manager = Arg("manager").ToLowerInvariant(); var package = Arg("package"); if (!System.Text.RegularExpressions.Regex.IsMatch(package, "^[A-Za-z0-9@._+/-]{1,160}$")) return "Nome de pacote inválido.";
                var command = manager switch { "winget" => $"winget install --id '{package}' --exact --accept-package-agreements --accept-source-agreements --disable-interactivity", "npm" => $"npm install '{package}'", "pip" => $"python -m pip install '{package}'", "dotnet" => $"dotnet add package '{package}'", _ => "" }; if (command.Length == 0) return "Gerenciador permitido: winget, npm, pip ou dotnet.";
                UpdateActivity(activity, "Instalando recurso necessário…"); AppendActivityOutput(activity, "> " + command); var result = await RunAgentCommand(command, 900, line => AppendActivityOutput(activity, line)); var action = $"Dependência {package} instalada via {manager} · código {result.ExitCode}"; AddActivityStep(activity, action); RecordProjectAction(action); return result.Output + $"\nCódigo de saída: {result.ExitCode}";
            }
            if (name is "android_devices" or "android_logcat" or "android_clear_logcat" or "android_install_apk")
            {
                var adb = FindAdbPath(); if (adb is null) return "ADB não encontrado. Reinstale o AirCode 0.4.0 ou instale Android SDK Platform-Tools."; var adbCall = $"& '{adb.Replace("'", "''")}'";
                if (name == "android_devices") { var command = adbCall + " devices -l"; AppendActivityOutput(activity, "> adb devices -l"); var result = await RunAgentCommand(command, 30, line => AppendActivityOutput(activity, line)); AddActivityStep(activity, "Dispositivos Android consultados"); return result.Output; }
                if (name == "android_clear_logcat") { var command = adbCall + " logcat -c"; AppendActivityOutput(activity, "> adb logcat -c"); var result = await RunAgentCommand(command, 30, line => AppendActivityOutput(activity, line)); AddActivityStep(activity, "Buffer do Logcat limpo"); RecordProjectAction("Logcat limpo"); return result.Output + $"\nCódigo de saída: {result.ExitCode}"; }
                if (name == "android_install_apk") { var apk = ResolveProjectPath(Arg("path")); var command = $"{adbCall} install -r '{apk.Replace("'", "''")}'"; AppendActivityOutput(activity, "> adb install -r " + Path.GetRelativePath(activeProject!, apk)); var result = await RunAgentCommand(command, 300, line => AppendActivityOutput(activity, line)); AddActivityStep(activity, "Instalação do APK concluída"); RecordProjectAction("APK instalado: " + Path.GetRelativePath(activeProject!, apk)); return result.Output + $"\nCódigo de saída: {result.ExitCode}"; }
                var lines = Math.Clamp(IntArg("lines", 500), 20, 20_000); var filter = Arg("filter"); var logcatCommand = adbCall + (BoolArg("follow") ? " logcat" : $" logcat -d -t {lines}"); if (!string.IsNullOrWhiteSpace(filter)) logcatCommand += $" | Select-String -SimpleMatch '{filter.Replace("'", "''")}'";
                if (BoolArg("follow")) return await ExecuteProjectTool("start_process", JsonSerializer.Serialize(new { command = logcatCommand }), activity);
                AppendActivityOutput(activity, "> adb logcat -d -t " + lines + (string.IsNullOrWhiteSpace(filter) ? "" : " · filtro: " + filter)); var logResult = await RunAgentCommand(logcatCommand, 90, line => AppendActivityOutput(activity, line)); AddActivityStep(activity, $"Logcat lido ({lines} linhas solicitadas)"); return logResult.Output;
            }
            return "Ferramenta desconhecida: " + name;
        }
        catch (Exception ex) { AddActivityStep(activity, "Falha em " + ToolDisplayName(name) + ": " + ex.Message); return "Erro: " + ex.Message; }
    }
    void AddMessage(string who, string text, bool user)
    {
        var content = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap, Foreground = new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString("#ECEEEF")), FontSize = 14, LineHeight = 22 };
        var border = new Border { Background = user ? new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString("#292B2C")) : Media.Brushes.Transparent, BorderBrush = user ? new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString("#3A3D3F")) : Media.Brushes.Transparent, BorderThickness = user ? new Thickness(1) : new Thickness(0), CornerRadius = new CornerRadius(12), Padding = user ? new Thickness(15, 12, 15, 12) : new Thickness(0, 10, 0, 14), Margin = user ? new Thickness(120, 8, 0, 8) : new Thickness(0, 4, 70, 4), Child = content };
        Messages.Children.Add(border); ChatScroll.ScrollToEnd();
    }
    void AddImageMessage(string path, string prompt)
    {
        if (!File.Exists(path)) { AddMessage(ProductName, "A imagem deste histórico não está mais disponível em: " + path, false); return; }
        var bitmap = new System.Windows.Media.Imaging.BitmapImage(); bitmap.BeginInit(); bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad; bitmap.UriSource = new Uri(path); bitmap.EndInit(); bitmap.Freeze();
        var preview = new System.Windows.Controls.Image { Source = bitmap, MaxWidth = 650, MaxHeight = 520, Stretch = Media.Stretch.Uniform, HorizontalAlignment = System.Windows.HorizontalAlignment.Left };
        var imageBorder = new Border { Background = new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString("#181B1D")), BorderBrush = new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString("#343A3E")), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(13), Padding = new Thickness(5), Child = preview };
        var title = new TextBlock { Text = "Imagem criada localmente", Foreground = new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString("#ECEEEF")), FontSize = 14, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 9) };
        var caption = new TextBlock { Text = prompt, TextWrapping = TextWrapping.Wrap, Foreground = new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString("#929A9E")), FontSize = 11, Margin = new Thickness(0, 9, 0, 8), MaxWidth = 650 };
        var open = new System.Windows.Controls.Button { Content = "Abrir imagem", Padding = new Thickness(11, 6, 11, 6), Margin = new Thickness(0, 0, 8, 0), FontSize = 11 };
        open.Click += (_, _) => Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        var folder = new System.Windows.Controls.Button { Content = "Mostrar na pasta", Padding = new Thickness(11, 6, 11, 6), FontSize = 11, Background = Media.Brushes.Transparent };
        folder.Click += (_, _) => Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"") { UseShellExecute = true });
        var actions = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal, Children = { open, folder } };
        var panel = new StackPanel { Children = { title, imageBorder, caption, actions } };
        Messages.Children.Add(new Border { Background = Media.Brushes.Transparent, Padding = new Thickness(0, 10, 0, 16), Margin = new Thickness(0, 4, 70, 4), Child = panel }); ChatScroll.ScrollToEnd();
    }
    async Task GenerateChatImageAsync(string originalPrompt, string imagePrompt)
    {
        isSending = true; generationCancellation = new CancellationTokenSource(); SendButton.Content = "■"; SendButton.ToolTip = "Parar geração"; ChatImageButton.IsEnabled = false;
        var activity = BeginActivity("Preparando geração de imagem local");
        try
        {
            AddActivityStep(activity, "Verificando ComfyUI, DirectML e modelo de imagem");
            if (!await EnsureMediaReady()) return;
            generationCancellation.Token.ThrowIfCancellationRequested(); UpdateActivity(activity, "Gerando imagem com IA local…"); AddActivityStep(activity, "Processamento iniciado na GPU/CPU disponível");
            var path = await GenerateLocalImageAsync(imagePrompt, generationCancellation.Token, status => UpdateActivity(activity, status));
            mediaResultPath = path; AddImageMessage(path, imagePrompt); AddActivityStep(activity, "Imagem salva em " + path); FinishActivity(activity, "Concluído");
            history.Add(new() { ["role"] = "user", ["content"] = originalPrompt });
            history.Add(new() { ["role"] = "assistant", ["content"] = "Imagem criada localmente: " + Path.GetFileName(path), ["type"] = "image", ["path"] = path, ["prompt"] = imagePrompt });
            saved.Chats[ChatKey] = history.Select(item => new Dictionary<string, string>(item)).ToList(); SaveState();
        }
        catch (OperationCanceledException) { FinishActivity(activity, "Interrompido", true); }
        catch (Exception ex) { FinishActivity(activity, "Falha", true); AddMessage(ProductName, "Não consegui gerar a imagem: " + ex.Message, false); }
        finally { generationCancellation?.Dispose(); generationCancellation = null; isSending = false; SendButton.Content = "↑"; SendButton.ToolTip = "Enviar"; ChatImageButton.IsEnabled = true; PromptBox.Focus(); }
    }
    async void CheckMediaEngine_Click(object sender, RoutedEventArgs e) => await CheckMediaEngine(true);
    async Task<bool> CheckMediaEngine(bool showFailure)
    {
        var endpoint = MediaEndpointBox.Text.Trim().TrimEnd('/'); if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri) || !endpointUri.IsLoopback) { MediaEngineStatus.Text = "Use somente um endereço local"; if (showFailure) System.Windows.MessageBox.Show("Por privacidade, o Estúdio aceita apenas ComfyUI neste computador (127.0.0.1 ou localhost).", "Processamento local"); return false; }
        try { using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) }; using var response = await client.GetAsync(endpoint + "/system_stats"); response.EnsureSuccessStatusCode(); MediaEngineDot.Fill = new Media.SolidColorBrush(Media.Color.FromRgb(74, 211, 153)); MediaEngineStatus.Text = "ComfyUI conectado · " + saved.MediaDevice; saved.MediaEndpoint = endpoint; SaveState(); return true; }
        catch (Exception ex) { MediaEngineDot.Fill = new Media.SolidColorBrush(Media.Color.FromRgb(104, 121, 115)); MediaEngineStatus.Text = "Preparação automática no primeiro clique em Gerar"; if (showFailure) System.Windows.MessageBox.Show("O motor local ainda não está ativo. Clique em Gerar e o AirCode preparará tudo automaticamente.\n\n" + ex.Message, "Estúdio de mídia"); return false; }
    }
    async void SetupMediaEngine_Click(object sender, RoutedEventArgs e)
    {
        MediaProgressBar.Visibility = Visibility.Visible; try { await EnsureMediaReady(); } catch (Exception ex) { MediaProgressText.Text = "Falha na preparação automática"; System.Windows.MessageBox.Show(ex.Message, "Estúdio local"); } finally { MediaProgressBar.Visibility = Visibility.Collapsed; }
    }
    async Task<bool> EnsureMediaReady()
    {
        saved.MediaDevice = DetectMediaDevice();
        if (string.IsNullOrWhiteSpace(saved.ComfyPath)) saved.ComfyPath = Path.Combine(root, "MediaEngine", "ComfyUI");
        var marker = Path.Combine(saved.ComfyPath, ".aircode-ready");
        if (!File.Exists(marker) || !File.Exists(Path.Combine(saved.ComfyPath, ".venv", "Scripts", "python.exe"))) await InstallMediaEngineAsync(marker);
        PatchDirectMlCompatibility();
        await EnsureDefaultImageModelAsync();
        if (!await CheckMediaEngine(false)) StartMediaEngine();
        for (var attempt = 0; attempt < 45; attempt++) { if (await CheckMediaEngine(false)) { MediaProgressText.Text = "Estúdio local pronto"; return true; } await Task.Delay(1000); }
        throw new InvalidOperationException("O motor local foi instalado, mas não iniciou. Reinicie o AirCode e tente novamente.");
    }
    void PatchDirectMlCompatibility()
    {
        if (!saved.MediaDevice.StartsWith("AMD", StringComparison.OrdinalIgnoreCase)) return; var kitchen = Path.Combine(saved.ComfyPath, ".venv", "Lib", "site-packages", "comfy_kitchen"); if (!Directory.Exists(kitchen)) return; foreach (var path in Directory.EnumerateFiles(kitchen, "*.py", SearchOption.AllDirectories)) { var source = File.ReadAllText(path); var patched = source.Replace("from __future__ import annotations\r\n", "").Replace("from __future__ import annotations\n", ""); if (path.EndsWith(Path.Combine("eager", "na.py"), StringComparison.OrdinalIgnoreCase)) patched = patched.Replace("import math\r\n", "import math\r\nfrom typing import List, Optional\r\n").Replace("import math\n", "import math\nfrom typing import List, Optional\n").Replace("kernel_size: list[int]", "kernel_size: List[int]").Replace("is_causal: list[bool] | None", "is_causal: Optional[List[bool]]").Replace("is_causal: list[bool]", "is_causal: List[bool]"); if (patched != source) File.WriteAllText(path, patched); }
    }
    static string DetectMediaDevice()
    {
        try { using var process = new Process { StartInfo = CreatePowerShellStartInfo("(Get-CimInstance Win32_VideoController | Select-Object -ExpandProperty Name) -join ';'", Environment.CurrentDirectory) }; process.Start(); var names = process.StandardOutput.ReadToEnd(); process.WaitForExit(8000); if (names.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase)) return "NVIDIA CUDA"; if (names.Contains("Intel", StringComparison.OrdinalIgnoreCase) && (names.Contains("Arc", StringComparison.OrdinalIgnoreCase) || names.Contains("Iris", StringComparison.OrdinalIgnoreCase))) return "Intel Arc / XPU"; if (names.Contains("AMD", StringComparison.OrdinalIgnoreCase) || names.Contains("Radeon", StringComparison.OrdinalIgnoreCase)) return "AMD DirectML"; } catch { } return "CPU (compatibilidade)";
    }
    async Task InstallMediaEngineAsync(string marker)
    {
        var comfyPath = saved.ComfyPath; var selected = Directory.GetParent(comfyPath)?.FullName ?? Path.Combine(root, "MediaEngine"); Directory.CreateDirectory(selected); MediaProgressText.Text = "Instalando o motor local automaticamente…";
        var device = saved.MediaDevice; var torchCommand = device.StartsWith("NVIDIA") ? "pip install torch torchvision torchaudio --extra-index-url https://download.pytorch.org/whl/cu130" : device.StartsWith("Intel") ? "pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/xpu" : device.StartsWith("CPU") ? "pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cpu" : "pip install torch-directml"; var directMlAudio = device.StartsWith("AMD") ? "; & '.venv\\Scripts\\python.exe' -m pip install torchaudio==2.4.1 torchvision==0.19.1 --index-url https://download.pytorch.org/whl/cpu --no-deps" : ""; var deviceInstall = $"; & '.venv\\Scripts\\python.exe' -m pip uninstall -y torch torchvision torchaudio; & '.venv\\Scripts\\python.exe' -m {torchCommand}{directMlAudio}";
        var escaped = comfyPath.Replace("'", "''"); var command = $"$ErrorActionPreference='Stop'; if (!(Test-Path '{escaped}\\main.py')) {{ git clone --depth 1 https://github.com/Comfy-Org/ComfyUI.git '{escaped}' }}; Set-Location '{escaped}'; if (!(Test-Path '.venv\\Scripts\\python.exe')) {{ py -3.12 -m venv .venv }}; & '.venv\\Scripts\\python.exe' -m pip install --upgrade pip; & '.venv\\Scripts\\python.exe' -m pip install -r requirements.txt{deviceInstall}; New-Item -ItemType File -Force '{marker.Replace("'", "''")}' | Out-Null";
        using var process = new Process { StartInfo = CreatePowerShellStartInfo(command, selected) }; process.Start();
        async Task ReadSetup(StreamReader reader) { string? line; while ((line = await reader.ReadLineAsync()) is not null) await Dispatcher.InvokeAsync(() => MediaProgressText.Text = TrimText(line, 140)); }
        var output = ReadSetup(process.StandardOutput); var error = ReadSetup(process.StandardError); await process.WaitForExitAsync(); await Task.WhenAll(output, error); if (process.ExitCode != 0) throw new InvalidOperationException("Não foi possível instalar o motor local. O instalador precisa de Python 3.12, Git e acesso ao Hugging Face."); SaveState();
    }
    async Task EnsureDefaultImageModelAsync()
    {
        const string fileName = "v1-5-pruned-emaonly-fp16.safetensors"; const string url = "https://huggingface.co/Comfy-Org/stable-diffusion-v1-5-archive/resolve/main/v1-5-pruned-emaonly-fp16.safetensors?download=true";
        var folder = Path.Combine(saved.ComfyPath, "models", "checkpoints"); Directory.CreateDirectory(folder); var target = Path.Combine(folder, fileName); if (File.Exists(target) && new FileInfo(target).Length > 2_000_000_000) return;
        var partial = target + ".part"; var existing = File.Exists(partial) ? new FileInfo(partial).Length : 0L; MediaProgressText.Text = "Baixando o modelo de imagem local · 2,13 GB…"; using var client = new HttpClient { Timeout = Timeout.InfiniteTimeSpan }; using var request = new HttpRequestMessage(HttpMethod.Get, url); if (existing > 0) request.Headers.Range = new RangeHeaderValue(existing, null); using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead); response.EnsureSuccessStatusCode(); var resumed = response.StatusCode == System.Net.HttpStatusCode.PartialContent; if (!resumed) existing = 0; var total = existing + (response.Content.Headers.ContentLength ?? 0); await using var input = await response.Content.ReadAsStreamAsync(); await using var output = new FileStream(partial, resumed ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read, 1024 * 1024, true); var buffer = new byte[1024 * 1024]; long downloaded = existing; var clock = Stopwatch.StartNew();
        while (true) { var read = await input.ReadAsync(buffer); if (read == 0) break; await output.WriteAsync(buffer.AsMemory(0, read)); downloaded += read; if (clock.ElapsedMilliseconds % 500 < 40) { var percent = total > 0 ? downloaded * 100d / total : 0; MediaProgressText.Text = total > 0 ? $"Preparando modelo local · {percent:0}% de 2,13 GB" : $"Preparando modelo local · {FormatBytes(downloaded)}"; } }
        await output.FlushAsync(); output.Close(); File.Move(partial, target, true); MediaProgressText.Text = "Modelo local instalado";
    }
    void StartMediaEngine()
    {
        if (mediaServer is { HasExited: false } || !Directory.Exists(saved.ComfyPath)) return; var python = Path.Combine(saved.ComfyPath, ".venv", "Scripts", "python.exe"); var main = Path.Combine(saved.ComfyPath, "main.py"); if (!File.Exists(python) || !File.Exists(main)) return;
        var info = new ProcessStartInfo(python) { WorkingDirectory = saved.ComfyPath, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true }; string[] arguments = saved.MediaDevice.StartsWith("CPU", StringComparison.OrdinalIgnoreCase) ? [main, "--cpu", "--listen", "127.0.0.1", "--port", "8188"] : saved.MediaDevice.StartsWith("AMD", StringComparison.OrdinalIgnoreCase) ? [main, "--directml", "--listen", "127.0.0.1", "--port", "8188", "--lowvram"] : [main, "--listen", "127.0.0.1", "--port", "8188"]; foreach (var value in arguments) info.ArgumentList.Add(value); mediaServer = new Process { StartInfo = info }; mediaServer.Start();
    }
    void MediaDeviceBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (loadingSettings || MediaDeviceBox.SelectedItem is not ComboBoxItem item) return; saved.MediaDevice = item.Content?.ToString() ?? saved.MediaDevice; SetupMediaButton.Content = "Configurar " + saved.MediaDevice.Split(' ')[0]; SaveState(); }
    void MediaTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MediaWorkflowBox is null || MediaTypeBox.SelectedItem is not ComboBoxItem item) return; var video = item.Content?.ToString() == "Vídeo"; MediaWorkflowBox.Text = video ? saved.VideoWorkflow : saved.ImageWorkflow; MediaWorkflowHint.Text = video ? "Vídeo local criado por uma sequência coerente de quadros gerados pela IA." : "Imagem local pronta para gerar; o primeiro uso prepara tudo sozinho."; MediaDurationPanel.Visibility = video ? Visibility.Visible : Visibility.Collapsed; MediaFormatBox.SelectedIndex = video ? 2 : Math.Min(MediaFormatBox.SelectedIndex, 1); GenerateMediaButton.Content = video ? "✦ Gerar vídeo" : "✦ Gerar imagem";
    }
    void BrowseMediaWorkflow_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new Forms.OpenFileDialog { Title = "Selecione um workflow salvo em formato API", Filter = "Workflow ComfyUI (*.json)|*.json" }; if (dialog.ShowDialog() != Forms.DialogResult.OK) return; MediaWorkflowBox.Text = dialog.FileName; var video = (MediaTypeBox.SelectedItem as ComboBoxItem)?.Content?.ToString() == "Vídeo"; if (video) saved.VideoWorkflow = dialog.FileName; else saved.ImageWorkflow = dialog.FileName; SaveState();
    }
    async void GenerateMedia_Click(object sender, RoutedEventArgs e)
    {
        var prompt = MediaPromptBox.Text.Trim(); if (string.IsNullOrWhiteSpace(prompt)) { System.Windows.MessageBox.Show("Descreva o que deseja gerar.", "Estúdio de mídia"); return; }
        var video = (MediaTypeBox.SelectedItem as ComboBoxItem)?.Content?.ToString() == "Vídeo";
        GenerateMediaButton.IsEnabled = false; MediaProgressBar.Visibility = Visibility.Visible; MediaProgressText.Text = video ? "Gerando vídeo · isso pode levar vários minutos…" : "Gerando imagem…"; MediaEmptyText.Visibility = Visibility.Collapsed;
        try
        {
            if (!await EnsureMediaReady()) return;
            if (video) { mediaResultPath = await GenerateLocalVideoAsync(prompt); ShowMediaResult(mediaResultPath); MediaProgressText.Text = "Concluído · " + Path.GetFileName(mediaResultPath); OpenMediaResultButton.IsEnabled = true; return; }
            using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) }; var endpoint = MediaEndpointBox.Text.Trim().TrimEnd('/'); JsonElement workflow;
            if (File.Exists(MediaWorkflowBox.Text.Trim())) { var raw = await File.ReadAllTextAsync(MediaWorkflowBox.Text.Trim()); raw = raw.Replace("{{prompt}}", JsonEncodedText.Encode(prompt).ToString()).Replace("{{negative_prompt}}", JsonEncodedText.Encode(MediaNegativeBox.Text).ToString()).Replace("{{seed}}", Random.Shared.NextInt64(1, long.MaxValue).ToString()); using var document = JsonDocument.Parse(raw); workflow = document.RootElement.Clone(); }
            else workflow = await BuildDefaultImageWorkflow(client, endpoint, prompt, MediaNegativeBox.Text);
            EnsureLocalWorkflow(workflow); var requestBody = JsonSerializer.Serialize(new { prompt = workflow, client_id = "aircode-local" }); using var response = await client.PostAsync(endpoint + "/prompt", new StringContent(requestBody, Encoding.UTF8, "application/json")); var responseText = await response.Content.ReadAsStringAsync(); response.EnsureSuccessStatusCode(); using var submitted = JsonDocument.Parse(responseText); var promptId = submitted.RootElement.GetProperty("prompt_id").GetString() ?? throw new InvalidOperationException("ComfyUI não retornou o ID da tarefa.");
            var output = await WaitForMediaOutput(client, endpoint, promptId); var query = $"filename={Uri.EscapeDataString(output.FileName)}&subfolder={Uri.EscapeDataString(output.Subfolder)}&type={Uri.EscapeDataString(output.Type)}"; var bytes = await client.GetByteArrayAsync(endpoint + "/view?" + query); var mediaFolder = Path.Combine(root, "geracoes"); Directory.CreateDirectory(mediaFolder); var requestedFormat = (MediaFormatBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "PNG"; var extension = requestedFormat == "JPG" ? ".jpg" : ".png"; mediaResultPath = Path.Combine(mediaFolder, $"aircode-{DateTime.Now:yyyyMMdd-HHmmss}{extension}"); await SaveGeneratedImageAsync(bytes, mediaResultPath, requestedFormat); ShowMediaResult(mediaResultPath); MediaProgressText.Text = "Concluído · " + Path.GetFileName(mediaResultPath); OpenMediaResultButton.IsEnabled = true;
        }
        catch (Exception ex) { MediaProgressText.Text = "Falha na geração"; MediaEmptyText.Visibility = Visibility.Visible; System.Windows.MessageBox.Show(ex.Message, "Erro ao gerar mídia"); }
        finally { GenerateMediaButton.IsEnabled = true; MediaProgressBar.Visibility = Visibility.Collapsed; }
    }
    async Task<string> GenerateLocalImageAsync(string prompt, CancellationToken cancellationToken = default, Action<string>? progress = null)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) }; var endpoint = MediaEndpointBox.Text.Trim().TrimEnd('/');
        var workflow = await BuildDefaultImageWorkflow(client, endpoint, prompt, MediaNegativeBox.Text); EnsureLocalWorkflow(workflow);
        var requestBody = JsonSerializer.Serialize(new { prompt = workflow, client_id = "air-ia-code-chat" });
        using var response = await client.PostAsync(endpoint + "/prompt", new StringContent(requestBody, Encoding.UTF8, "application/json"), cancellationToken); var responseText = await response.Content.ReadAsStringAsync(cancellationToken); response.EnsureSuccessStatusCode();
        using var submitted = JsonDocument.Parse(responseText); var promptId = submitted.RootElement.GetProperty("prompt_id").GetString() ?? throw new InvalidOperationException("ComfyUI não retornou o ID da tarefa.");
        var output = await WaitForMediaOutput(client, endpoint, promptId, cancellationToken, progress); var query = $"filename={Uri.EscapeDataString(output.FileName)}&subfolder={Uri.EscapeDataString(output.Subfolder)}&type={Uri.EscapeDataString(output.Type)}"; var bytes = await client.GetByteArrayAsync(endpoint + "/view?" + query, cancellationToken);
        var mediaFolder = Path.Combine(root, "geracoes"); Directory.CreateDirectory(mediaFolder); var resultPath = Path.Combine(mediaFolder, $"air-ia-code-{DateTime.Now:yyyyMMdd-HHmmss}.png"); await File.WriteAllBytesAsync(resultPath, bytes, cancellationToken); return resultPath;
    }
    async Task<JsonElement> BuildDefaultImageWorkflow(HttpClient client, string endpoint, string prompt, string negative, long? seedOverride = null)
    {
        var infoText = await client.GetStringAsync(endpoint + "/object_info/CheckpointLoaderSimple"); using var info = JsonDocument.Parse(infoText); var names = new List<string>(); CollectStringsUnderProperty(info.RootElement, "ckpt_name", names); var checkpoint = names.FirstOrDefault(name => name.EndsWith(".safetensors", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".ckpt", StringComparison.OrdinalIgnoreCase)); if (checkpoint is null) throw new InvalidOperationException("Nenhum checkpoint de imagem foi encontrado no ComfyUI. Instale um modelo SD 1.5/SDXL ou selecione um workflow que faça o download do modelo.");
        var resolution = (MediaResolutionBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "512 × 512"; var dimensions = System.Text.RegularExpressions.Regex.Matches(resolution, "\\d+").Select(match => int.Parse(match.Value)).ToArray(); var width = dimensions.ElementAtOrDefault(0); var height = dimensions.ElementAtOrDefault(1); if (width < 256) width = 512; if (height < 256) height = 512; var quality = (MediaQualityBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Equilibrada"; var steps = quality == "Rápida" ? 14 : quality == "Alta" ? 32 : 22;
        var workflow = new Dictionary<string, object> { ["1"] = new { class_type = "CheckpointLoaderSimple", inputs = new { ckpt_name = checkpoint } }, ["2"] = new { class_type = "CLIPTextEncode", inputs = new { text = prompt, clip = new object[] { "1", 1 } } }, ["3"] = new { class_type = "CLIPTextEncode", inputs = new { text = negative, clip = new object[] { "1", 1 } } }, ["4"] = new { class_type = "EmptyLatentImage", inputs = new { width, height, batch_size = 1 } }, ["5"] = new { class_type = "KSampler", inputs = new { seed = seedOverride ?? Random.Shared.NextInt64(1, long.MaxValue), steps, cfg = 7.0, sampler_name = "dpmpp_2m", scheduler = "karras", denoise = 1.0, model = new object[] { "1", 0 }, positive = new object[] { "2", 0 }, negative = new object[] { "3", 0 }, latent_image = new object[] { "4", 0 } } }, ["6"] = new { class_type = "VAEDecode", inputs = new { samples = new object[] { "5", 0 }, vae = new object[] { "1", 2 } } }, ["7"] = new { class_type = "SaveImage", inputs = new { filename_prefix = "AirCode", images = new object[] { "6", 0 } } } }; using var doc = JsonDocument.Parse(JsonSerializer.Serialize(workflow)); return doc.RootElement.Clone();
    }
    async Task<string> GenerateLocalVideoAsync(string prompt)
    {
        var endpoint = MediaEndpointBox.Text.Trim().TrimEnd('/'); using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) }; var durationText = (MediaDurationBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "4 segundos"; var seconds = int.TryParse(new string(durationText.TakeWhile(char.IsDigit).ToArray()), out var parsed) ? parsed : 4; var frames = Math.Clamp(seconds * 6, 12, 36); var frameFolder = Path.Combine(root, "geracoes", "quadros-" + DateTime.Now.ToString("yyyyMMdd-HHmmss")); Directory.CreateDirectory(frameFolder); var seed = Random.Shared.NextInt64(1, long.MaxValue);
        for (var index = 0; index < frames; index++) { MediaProgressText.Text = $"Gerando vídeo local · quadro {index + 1} de {frames}"; var phase = index / (double)Math.Max(1, frames - 1); var framePrompt = $"{prompt}, coherent animation frame, same subject and scene, smooth motion, cinematic, motion phase {phase:0.00}"; var workflow = await BuildDefaultImageWorkflow(client, endpoint, framePrompt, MediaNegativeBox.Text, seed + index * 17); var body = JsonSerializer.Serialize(new { prompt = workflow, client_id = "aircode-local-video" }); using var response = await client.PostAsync(endpoint + "/prompt", new StringContent(body, Encoding.UTF8, "application/json")); var textResponse = await response.Content.ReadAsStringAsync(); response.EnsureSuccessStatusCode(); using var submitted = JsonDocument.Parse(textResponse); var id = submitted.RootElement.GetProperty("prompt_id").GetString()!; var output = await WaitForMediaOutput(client, endpoint, id); var query = $"filename={Uri.EscapeDataString(output.FileName)}&subfolder={Uri.EscapeDataString(output.Subfolder)}&type={Uri.EscapeDataString(output.Type)}"; var bytes = await client.GetByteArrayAsync(endpoint + "/view?" + query); await File.WriteAllBytesAsync(Path.Combine(frameFolder, $"frame-{index:0000}.png"), bytes); }
        var ffmpeg = FindExecutable("ffmpeg.exe") ?? throw new InvalidOperationException("FFmpeg não foi encontrado. Reinstale o AirCode para incluir o encoder de vídeo."); var resultPath = Path.Combine(root, "geracoes", $"aircode-{DateTime.Now:yyyyMMdd-HHmmss}.mp4"); var info = new ProcessStartInfo(ffmpeg) { WorkingDirectory = frameFolder, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true }; foreach (var argument in new[] { "-y", "-framerate", "6", "-i", "frame-%04d.png", "-c:v", "libx264", "-pix_fmt", "yuv420p", "-movflags", "+faststart", resultPath }) info.ArgumentList.Add(argument); using var process = new Process { StartInfo = info }; process.Start(); var error = await process.StandardError.ReadToEndAsync(); await process.WaitForExitAsync(); if (process.ExitCode != 0) throw new InvalidOperationException("Falha ao montar o vídeo local: " + TrimText(error, 2000)); try { Directory.Delete(frameFolder, true); } catch { } return resultPath;
    }
    static async Task SaveGeneratedImageAsync(byte[] source, string path, string format)
    {
        if (format != "JPG") { await File.WriteAllBytesAsync(path, source); return; } await Task.Run(() => { using var input = new MemoryStream(source); var decoder = System.Windows.Media.Imaging.BitmapDecoder.Create(input, System.Windows.Media.Imaging.BitmapCreateOptions.PreservePixelFormat, System.Windows.Media.Imaging.BitmapCacheOption.OnLoad); var encoder = new System.Windows.Media.Imaging.JpegBitmapEncoder { QualityLevel = 94 }; encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(decoder.Frames[0])); using var output = File.Create(path); encoder.Save(output); });
    }
    static string? FindExecutable(string name)
    {
        var candidates = (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries).Select(folder => Path.Combine(folder.Trim('"'), name)).ToList(); var packages = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "WinGet", "Packages"); if (Directory.Exists(packages)) try { candidates.AddRange(Directory.EnumerateFiles(packages, name, SearchOption.AllDirectories)); } catch { } return candidates.FirstOrDefault(File.Exists);
    }
    static void CollectStringsUnderProperty(JsonElement element, string propertyName, List<string> output, bool active = false)
    {
        if (element.ValueKind == JsonValueKind.Object) foreach (var property in element.EnumerateObject()) CollectStringsUnderProperty(property.Value, propertyName, output, active || property.NameEquals(propertyName)); else if (element.ValueKind == JsonValueKind.Array) foreach (var child in element.EnumerateArray()) CollectStringsUnderProperty(child, propertyName, output, active); else if (active && element.ValueKind == JsonValueKind.String) output.Add(element.GetString() ?? "");
    }
    static void EnsureLocalWorkflow(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object) { foreach (var property in element.EnumerateObject()) { if (property.NameEquals("class_type") && property.Value.ValueKind == JsonValueKind.String) { var node = property.Value.GetString() ?? ""; string[] remoteMarkers = ["api", "luma", "runway", "kling", "openai", "gemini", "ideogram", "recraft", "stabilityai", "bfl"]; if (remoteMarkers.Any(marker => node.Contains(marker, StringComparison.OrdinalIgnoreCase))) throw new InvalidOperationException($"O node '{node}' parece usar um serviço externo. O AirCode permite apenas geração local."); } EnsureLocalWorkflow(property.Value); } } else if (element.ValueKind == JsonValueKind.Array) foreach (var child in element.EnumerateArray()) EnsureLocalWorkflow(child);
    }
    sealed record MediaOutput(string FileName, string Subfolder, string Type);
    async Task<MediaOutput> WaitForMediaOutput(HttpClient client, string endpoint, string promptId, CancellationToken cancellationToken = default, Action<string>? progress = null)
    {
        for (var attempt = 0; attempt < 1800; attempt++) { await Task.Delay(1000, cancellationToken); var text = await client.GetStringAsync(endpoint + "/history/" + promptId, cancellationToken); using var historyDoc = JsonDocument.Parse(text); if (TryFindMediaOutput(historyDoc.RootElement, out var output)) return output; if (attempt % 5 == 0) { var status = $"Processando imagem local · {attempt + 1}s…"; MediaProgressText.Text = status; progress?.Invoke(status); } } throw new TimeoutException("O ComfyUI não concluiu a geração dentro do limite.");
    }
    static bool TryFindMediaOutput(JsonElement element, out MediaOutput output)
    {
        if (element.ValueKind == JsonValueKind.Object) { if (element.TryGetProperty("filename", out var filename) && filename.ValueKind == JsonValueKind.String) { output = new(filename.GetString() ?? "output.bin", element.TryGetProperty("subfolder", out var subfolder) ? subfolder.GetString() ?? "" : "", element.TryGetProperty("type", out var type) ? type.GetString() ?? "output" : "output"); return true; } foreach (var property in element.EnumerateObject()) if (TryFindMediaOutput(property.Value, out output)) return true; } else if (element.ValueKind == JsonValueKind.Array) foreach (var child in element.EnumerateArray()) if (TryFindMediaOutput(child, out output)) return true; output = null!; return false;
    }
    void ShowMediaResult(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant(); var video = extension is ".mp4" or ".webm" or ".avi" or ".mov" or ".gif"; MediaImagePreview.Visibility = video ? Visibility.Collapsed : Visibility.Visible; MediaVideoPreview.Visibility = video ? Visibility.Visible : Visibility.Collapsed; if (video) { MediaVideoPreview.Source = new Uri(path); MediaVideoPreview.Play(); } else { var bitmap = new System.Windows.Media.Imaging.BitmapImage(); bitmap.BeginInit(); bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad; bitmap.UriSource = new Uri(path); bitmap.EndInit(); MediaImagePreview.Source = bitmap; }
    }
    void OpenMediaResult_Click(object sender, RoutedEventArgs e) { if (File.Exists(mediaResultPath)) Process.Start(new ProcessStartInfo(mediaResultPath) { UseShellExecute = true }); }
    void QuickDeveloperTask_Click(object sender, RoutedEventArgs e)
    {
        if (activeProject is null || sender is not System.Windows.Controls.Button button) return;
        var request = button.Tag?.ToString() switch
        {
            "build" => "Inspecione o ambiente, compile o projeto agora, analise todos os erros e faça as correções necessárias. Compile novamente para validar.",
            "test" => "Detecte a estrutura de testes, execute todos os testes, investigue falhas e corrija o projeto. Rode os testes novamente para validar.",
            "run" => "Inspecione o projeto, compile se necessário e inicie a aplicação. Use um processo em segundo plano quando ela precisar continuar executando e informe PID e endereço.",
            "debug" => "Depure o projeto: reproduza o problema, execute build e testes, analise logs e exceções, aplique correções e valide o resultado completo.",
            _ => "Inspecione o projeto."
        };
        SetTab(ChatPage, Path.GetFileName(activeProject), "Agente de desenvolvimento · atividade em tempo real"); PromptBox.Text = request; Send_Click(PromptBox, e);
    }
    void AddProject_Click(object sender, RoutedEventArgs e) { using var d = new Forms.FolderBrowserDialog { Description = "Selecione a pasta do projeto" }; if (d.ShowDialog() == Forms.DialogResult.OK) { if (!projects.Contains(d.SelectedPath)) projects.Add(d.SelectedPath); RefreshProjectsSidebar(); OpenProject(d.SelectedPath); SaveState(); } }
    void RefreshProjectsSidebar() { SidebarProjects.ItemsSource = projects.Select(Path.GetFileName).ToList(); }
    void SidebarProjects_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (SidebarProjects.SelectedIndex >= 0 && SidebarProjects.SelectedIndex < projects.Count) { var path = projects[SidebarProjects.SelectedIndex]; if (!SamePath(path, activeProject)) OpenProject(path); } }
    string ChatKey => activeProject ?? "__general";
    void OpenProject(string path) { activeProject = path; saved.LastProject = path; ActiveProjectText.Text = Path.GetFileName(path); ProjectAccessText.Text = saved.ConfirmCommands ? "Acesso ao projeto · confirmar comandos" : "Acesso total · comandos automáticos"; ProjectAccessText.Foreground = new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString("#79D7AC")); var projectIndex = projects.IndexOf(path); if (projectIndex >= 0 && SidebarProjects.SelectedIndex != projectIndex) SidebarProjects.SelectedIndex = projectIndex; BuildTree(path); history.Clear(); if (saved.Chats.TryGetValue(ChatKey, out var chat)) history.AddRange(chat.Select(x => new Dictionary<string, string>(x))); RenderChat(); SetTab(ChatPage, Path.GetFileName(path), "Chat individual · ações e saídas exibidas em tempo real"); SaveState(); PromptBox.Focus(); }
    void RenderChat() { Messages.Children.Clear(); if (history.Count == 0) { Messages.Children.Add(new TextBlock { Text = "O que vamos construir?", FontSize = 24, FontWeight = FontWeights.SemiBold, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, Margin = new Thickness(0, 70, 0, 8) }); Messages.Children.Add(new TextBlock { Text = activeProject is null ? "Escolha um projeto e um modelo local para começar." : $"Contexto ativo: {Path.GetFileName(activeProject)}", Foreground = new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString("#858B8E")), HorizontalAlignment = System.Windows.HorizontalAlignment.Center }); return; } foreach (var message in history) { var user = message.GetValueOrDefault("role") == "user"; if (!user && message.GetValueOrDefault("type") == "image" && message.TryGetValue("path", out var path)) AddImageMessage(path, message.GetValueOrDefault("prompt", "Imagem gerada")); else AddMessage(user ? "Você" : ProductName, message.GetValueOrDefault("content", ""), user); } }
    void BuildTree(string path) { ProjectTree.Items.Clear(); var rootNode = CreateNode(path, 0); ProjectTree.Items.Add(rootNode); rootNode.IsExpanded = true; }
    TreeViewItem CreateNode(string path, int depth) { var node = new TreeViewItem { Header = Path.GetFileName(path), Tag = path }; if (Directory.Exists(path) && depth < 5) { try { var ignored = new[] { ".git", ".gradle", ".kotlin", ".idea", "bin", "obj", "node_modules", "build", "dist", "out", "release", ".cache" }; foreach (var dir in Directory.EnumerateDirectories(path).Where(x => !ignored.Contains(Path.GetFileName(x), StringComparer.OrdinalIgnoreCase)).Take(80)) node.Items.Add(CreateNode(dir, depth + 1)); foreach (var file in Directory.EnumerateFiles(path).Take(200)) node.Items.Add(new TreeViewItem { Header = Path.GetFileName(file), Tag = file }); } catch { } } return node; }
    void ProjectTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) { if (e.NewValue is TreeViewItem item && item.Tag is string path && File.Exists(path)) { try { var info = new FileInfo(path); if (info.Length > 2_000_000) { EditorBox.Text = "Arquivo grande demais para o editor interno."; return; } currentFile = path; CurrentFileText.Text = Path.GetFileName(path); EditorBox.Text = File.ReadAllText(path); } catch (Exception ex) { EditorBox.Text = ex.Message; } } }
    void SaveFile_Click(object sender, RoutedEventArgs e) { if (currentFile is null) return; try { File.WriteAllText(currentFile, EditorBox.Text); CurrentFileText.Text = Path.GetFileName(currentFile) + " · salvo"; } catch (Exception ex) { System.Windows.MessageBox.Show(ex.Message, "Falha ao salvar"); } }
    async void RunCommand_Click(object sender, RoutedEventArgs e) { await RunCommand(); }
    async void CommandBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) { if (e.Key == System.Windows.Input.Key.Enter) { e.Handled = true; await RunCommand(); } }
    async Task RunCommand()
    {
        var command = CommandBox.Text.Trim(); if (string.IsNullOrWhiteSpace(command) || activeProject is null) return; if (ConfirmCommandsBox.IsChecked == true && System.Windows.MessageBox.Show($"Executar no projeto?\n\n{command}", "Confirmar comando", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
        CommandBox.Clear(); TerminalOutput.Text += $"> {command}\n";
        try
        {
            using var process = new Process { StartInfo = CreatePowerShellStartInfo(command, activeProject) }; process.Start();
            async Task CopyLive(StreamReader reader, string prefix) { string? line; while ((line = await reader.ReadLineAsync()) is not null) await Dispatcher.InvokeAsync(() => TerminalOutput.Text += prefix + line + "\n"); }
            var outputTask = CopyLive(process.StandardOutput, ""); var errorTask = CopyLive(process.StandardError, "erro> "); await process.WaitForExitAsync(); await Task.WhenAll(outputTask, errorTask); TerminalOutput.Text += $"[código {process.ExitCode}]\n";
        }
        catch (Exception ex) { TerminalOutput.Text += "Falha ao executar: " + ex.Message + "\n"; }
    }
    void LoadState() { loadingSettings = true; Directory.CreateDirectory(root); try { if (File.Exists(StateFile)) saved = JsonSerializer.Deserialize<SavedState>(File.ReadAllText(StateFile)) ?? new(); } catch { saved = new(); } saved.Projects ??= new(); saved.Chats ??= new(); saved.ProjectMemories ??= new(); saved.ProjectActions ??= new(); if (!int.TryParse(saved.Context, out var contextSize) || contextSize < 2048) saved.Context = "8192"; foreach (var p in saved.Projects.Where(Directory.Exists)) projects.Add(p); foreach (ComboBoxItem item in ContextBox.Items) if (item.Content?.ToString() == saved.Context) item.IsSelected = true; foreach (ComboBoxItem item in PerformanceModeBox.Items) if (item.Content?.ToString() == saved.PerformanceMode) item.IsSelected = true; if (PerformanceModeBox.SelectedIndex < 0) PerformanceModeBox.SelectedIndex = 0; foreach (ComboBoxItem item in SpeedModeBox.Items) item.IsSelected = item.Tag?.ToString() == saved.PerformanceMode; if (SpeedModeBox.SelectedIndex < 0) SpeedModeBox.SelectedIndex = 0; GpuLayersBox.Text = saved.GpuLayers; TemperatureBox.Text = saved.Temperature; MaxTokensBox.Text = saved.MaxTokens.ToString(); ConfirmCommandsBox.IsChecked = saved.ConfirmCommands; AutoStartBox.IsChecked = saved.AutoStart; MediaEndpointBox.Text = saved.MediaEndpoint; foreach (ComboBoxItem item in MediaDeviceBox.Items) item.IsSelected = item.Content?.ToString() == saved.MediaDevice; if (MediaDeviceBox.SelectedIndex < 0) MediaDeviceBox.SelectedIndex = 0; MediaWorkflowBox.Text = saved.ImageWorkflow; SetupMediaButton.Content = "Configurar " + saved.MediaDevice.Split(' ')[0]; if (saved.Chats.TryGetValue(ChatKey, out var chat)) history.AddRange(chat); RenderChat(); loadingSettings = false; }
    void SpeedModeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (loadingSettings || SpeedModeBox.SelectedItem is not ComboBoxItem selected) return;
        var target = selected.Tag?.ToString();
        foreach (ComboBoxItem item in PerformanceModeBox.Items)
            if (item.Content?.ToString() == target) { PerformanceModeBox.SelectedItem = item; break; }
    }
    void PerformanceModeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (loadingSettings || PerformanceModeBox.SelectedItem is not ComboBoxItem item) return;
        var mode = item.Content?.ToString() ?? "Automático (recomendado)"; loadingSettings = true;
        (var context, var layers, var temperature, var tokens) = mode switch
        {
            "Velocidade máxima" => ("4096", "all", "0.20", "900"),
            "Equilibrado" => ("8192", "auto", "0.30", "1800"),
            "Qualidade máxima" => ("16384", "auto", "0.25", "3000"),
            "Personalizado" => (saved.Context, saved.GpuLayers, saved.Temperature, saved.MaxTokens.ToString()),
            _ => ("8192", "auto", "0.30", "1800")
        };
        foreach (ComboBoxItem contextItem in ContextBox.Items) contextItem.IsSelected = contextItem.Content?.ToString() == context;
        GpuLayersBox.Text = layers; TemperatureBox.Text = temperature; MaxTokensBox.Text = tokens;
        foreach (ComboBoxItem speedItem in SpeedModeBox.Items) speedItem.IsSelected = speedItem.Tag?.ToString() == mode;
        loadingSettings = false; SaveState();
    }
    void RunDiagnostics_Click(object sender, RoutedEventArgs e)
    {
        var engine = FindLlamaServer(); var adb = FindAdbPath();
        SystemStatusText.Text = $"Motor local: {(engine is null ? "não encontrado" : "pronto")}  ·  ADB/Logcat: {(adb is null ? "não encontrado" : "pronto")}  ·  CPU: {Environment.ProcessorCount} threads  ·  .NET {Environment.Version}";
        SystemStatusText.Foreground = new Media.SolidColorBrush((Media.Color)Media.ColorConverter.ConvertFromString(engine is null ? "#E6A36E" : "#79D7AC"));
    }
    void SettingsChanged(object sender, RoutedEventArgs e) { if (loadingSettings) return; if (sender is System.Windows.Controls.ComboBox or System.Windows.Controls.TextBox) { loadingSettings = true; PerformanceModeBox.SelectedIndex = 4; SpeedModeBox.SelectedIndex = 4; loadingSettings = false; } SaveState(); if (activeProject is not null) ProjectAccessText.Text = saved.ConfirmCommands ? "Acesso ao projeto · confirmar comandos" : "Acesso total · comandos automáticos"; }
    void SaveState() { saved.Projects = projects.ToList(); saved.Context = (ContextBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "8192"; saved.GpuLayers = GpuLayersBox.Text.Trim(); saved.Temperature = double.TryParse(TemperatureBox.Text.Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var temp) ? Math.Clamp(temp, 0, 2).ToString(System.Globalization.CultureInfo.InvariantCulture) : "0.30"; saved.MaxTokens = int.TryParse(MaxTokensBox.Text, out var max) ? Math.Clamp(max, 64, 32768) : 1800; saved.PerformanceMode = (PerformanceModeBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Automático (recomendado)"; saved.ConfirmCommands = ConfirmCommandsBox.IsChecked == true; saved.AutoStart = AutoStartBox.IsChecked == true; saved.MediaEndpoint = MediaEndpointBox.Text.Trim(); saved.MediaDevice = (MediaDeviceBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? saved.MediaDevice; Directory.CreateDirectory(root); File.WriteAllText(StateFile, JsonSerializer.Serialize(saved, new JsonSerializerOptions { WriteIndented = true })); }
    protected override void OnClosed(EventArgs e) { foreach (var process in agentProcesses.Values.ToList()) { try { if (!process.HasExited) process.Kill(true); } catch { } } agentProcesses.Clear(); try { if (mediaServer is { HasExited: false }) mediaServer.Kill(true); } catch { } StopOwnedServer(); base.OnClosed(e); }
}
