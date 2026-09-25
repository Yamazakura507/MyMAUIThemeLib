using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.EventArgs;
using ThemeForge.Abstractions.Interfaces;
using ThemeForge.Abstractions.Records;
using ThemeForge.Abstractions.Records.UseOfEffects;
using ThemeForge.Abstractions.Records.UseOfGeometry;
using ThemeForge.Abstractions.Records.UseOfTheme;
using ThemeForge.Abstractions.Records.UseOfTypograhy;
using ThemeForge.Maui.Controls.ComponentModels.Editors;
using ThemeForge.Maui.Controls.ComponentModels.Trees;
using ThemeForge.Maui.Controls.Helpers;
using ThemeForge.Maui.Studio.ComponentModels.Editors;
using ThemeForge.Maui.Studio.Enums;
using ThemeForge.Maui.Studio.Helpers;

namespace ThemeForge.Maui.Studio.ComponentModels
{
    /// <summary>
    /// Центральная ViewModel страницы настройки тем.
    /// </summary>
    public partial class ThemeStudioViewModel : ObservableObject
    {
        private readonly IThemeService themeService;
        private readonly IThemeRepository themeRepository;
        private readonly IControlThemeCatalog controlCatalog;
        private readonly IThemeFactory themeFactory;
        private readonly IThemeNamingService themeNamingService;

        private bool isLoadingFromDraft;
        private bool isUpdatingFromEditor;
        private bool isInitialized;

        private string? selectedControlType;
        private ComponentState selectedState = ComponentState.Default;
        private ThemeSettingGroup? selectedGroup;
        private ControlThemeDescriptor? selectedDescriptor;
        private bool isGlobalEditor;

        [ObservableProperty]
        private StudioMode mode = StudioMode.Ready;

        [ObservableProperty]
        private ThemeTreeViewModel? currentTree;

        [ObservableProperty]
        private ThemeTreeNodeViewModel? selectedNode;

        [ObservableProperty]
        private EditorKind activeEditor = EditorKind.None;

        [ObservableProperty]
        private ThemeDefinition? draft;

        [ObservableProperty]
        private PreviewMode previewMode = PreviewMode.Active;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        /// <summary>
        /// Дерево готовых тем.
        /// </summary>
        public ThemeTreeViewModel ReadyTree { get; }

        /// <summary>
        /// Дерево кастомных настроек.
        /// </summary>
        public ThemeTreeViewModel CustomTree { get; }

        /// <summary>
        /// Редактор простой темы.
        /// </summary>
        public SolidThemeEditorViewModel SolidEditor { get; }

        /// <summary>
        /// Редактор градиентной темы.
        /// </summary>
        public GradientThemeEditorViewModel GradientEditor { get; }

        /// <summary>
        /// Редактор типографики.
        /// </summary>
        public TypographyEditorViewModel TypographyEditor { get; }

        /// <summary>
        /// Редактор геометрии.
        /// </summary>
        public GeometryEditorViewModel GeometryEditor { get; }

        /// <summary>
        /// Редактор цветов компонента.
        /// </summary>
        public ComponentThemeEditorViewModel ComponentEditor { get; }

        /// <summary>
        /// Редактор эффектов.
        /// </summary>
        public EffectSettingsEditorViewModel EffectEditor { get; }

        /// <summary>
        /// Доступные режимы предпросмотра.
        /// </summary>
        public Array PreviewModes { get; } = Enum.GetValues(typeof(PreviewMode));

        /// <summary>
        /// Есть ли несохраненные изменения.
        /// </summary>
        public bool HasUnsavedChanges => themeService.HasUnsavedChanges;

        /// <summary>
        /// Создает ViewModel студии.
        /// </summary>
        public ThemeStudioViewModel(
            IThemeService themeService,
            IThemeRepository themeRepository,
            IControlThemeCatalog controlCatalog,
            IThemeFactory themeFactory,
            IThemeNamingService themeNamingService,
            SolidThemeEditorViewModel solidEditor,
            GradientThemeEditorViewModel gradientEditor,
            TypographyEditorViewModel typographyEditor,
            GeometryEditorViewModel geometryEditor,
            ComponentThemeEditorViewModel componentEditor,
            EffectSettingsEditorViewModel effectEditor)
        {
            this.themeService = themeService;
            this.themeRepository = themeRepository;
            this.controlCatalog = controlCatalog;
            this.themeFactory = themeFactory;
            this.themeNamingService = themeNamingService;

            SolidEditor = solidEditor;
            GradientEditor = gradientEditor;
            TypographyEditor = typographyEditor;
            GeometryEditor = geometryEditor;
            ComponentEditor = componentEditor;
            EffectEditor = effectEditor;

            ReadyTree = new ThemeTreeViewModel();
            CustomTree = new ThemeTreeViewModel();

            Draft = this.themeService.Draft;
            CurrentTree = ReadyTree;

            this.themeService.DraftChanged += OnThemeServiceDraftChanged;
            this.themeService.Applied += OnThemeServiceApplied;
            this.themeRepository.Changed += OnThemeRepositoryChanged;

            SolidEditor.PropertyChanged += OnEditorPropertyChanged;
            GradientEditor.PropertyChanged += OnEditorPropertyChanged;
            TypographyEditor.PropertyChanged += OnEditorPropertyChanged;
            GeometryEditor.PropertyChanged += OnEditorPropertyChanged;
            ComponentEditor.PropertyChanged += OnEditorPropertyChanged;
            EffectEditor.PropertyChanged += OnEditorPropertyChanged;
        }

        partial void OnModeChanged(StudioMode value)
        {
            SelectedNode = null;
            ActiveEditor = EditorKind.None;

            CurrentTree = value switch
            {
                StudioMode.Ready => ReadyTree,
                StudioMode.Custom => CustomTree,
                _ => ReadyTree
            };
        }

        partial void OnSelectedNodeChanged(ThemeTreeNodeViewModel? value)
        {
            if (value is null)
            {
                ActiveEditor = EditorKind.None;
                return;
            }

            switch (value.Kind)
            {
                case TreeNodeKind.Preset:
                    if (value.Theme is not null)
                    {
                        _ = SelectPresetAsync(value.Theme.Id);
                    }
                    ActiveEditor = EditorKind.None;
                    break;
                case TreeNodeKind.SettingGroup:
                    ActivateSettingGroup(value);
                    break;
                default:
                    ActiveEditor = EditorKind.None;
                    break;
            }
        }

        partial void OnPreviewModeChanged(PreviewMode value)
        {
            StatusMessage = value switch
            {
                PreviewMode.Active => "Предпросмотр: активный интерфейс",
                PreviewMode.Disabled => "Предпросмотр: неактивный интерфейс",
                _ => "Предпросмотр: рабочий интерфейс"
            };
        }

        /// <summary>
        /// Инициализирует студию.
        /// </summary>
        [RelayCommand]
        private async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (isInitialized) return;

            isInitialized = true;

            await ReloadTreesAsync(cancellationToken);

            Draft = themeService.Draft;
            LoadEditorsFromDraft();

            StatusMessage = "Студия тем готова";
        }

        /// <summary>
        /// Переключает режим Ready.
        /// </summary>
        [RelayCommand]
        private void SwitchToReady()
        {
            Mode = StudioMode.Ready;
        }

        /// <summary>
        /// Переключает режим Custom.
        /// </summary>
        [RelayCommand]
        private void SwitchToCustom()
        {
            Mode = StudioMode.Custom;
        }

        /// <summary>
        /// Открывает редактор простой темы.
        /// </summary>
        [RelayCommand]
        private void NewSolidTheme()
        {
            Mode = StudioMode.Ready;
            ActiveEditor = EditorKind.AddSolid;

            StatusMessage = "Создание простой цветовой темы";
        }

        /// <summary>
        /// Открывает редактор градиентной темы.
        /// </summary>
        [RelayCommand]
        private void NewGradientTheme()
        {
            Mode = StudioMode.Ready;
            ActiveEditor = EditorKind.AddGradient;

            StatusMessage = "Создание градиентной темы";
        }

        /// <summary>
        /// Добавляет текущую простую тему в пресеты.
        /// </summary>
        [RelayCommand]
        private async Task AddSolidThemeToPresetsAsync(CancellationToken cancellationToken)
        {
            if (SolidEditor.PreviewTheme is null)
            {
                await SolidEditor.GenerateCommand.ExecuteAsync(null);
            }

            if (SolidEditor.PreviewTheme is null)
            {
                StatusMessage = "Не удалось создать простую тему";
                return;
            }

            await AddThemeAsPresetAsync(SolidEditor.PreviewTheme, cancellationToken);
        }

        /// <summary>
        /// Добавляет текущую градиентную тему в пресеты.
        /// </summary>
        [RelayCommand]
        private async Task AddGradientThemeToPresetsAsync(CancellationToken cancellationToken)
        {
            if (GradientEditor.PreviewTheme is null)
            {
                await GradientEditor.BuildCommand.ExecuteAsync(null);
            }

            if (GradientEditor.PreviewTheme is null)
            {
                StatusMessage = "Не удалось создать градиентную тему";
                return;
            }

            await AddThemeAsPresetAsync(GradientEditor.PreviewTheme, cancellationToken);
        }

        /// <summary>
        /// Применяет текущий черновик к интерфейсу.
        /// </summary>
        [RelayCommand]
        private async Task ApplyDraftAsync(CancellationToken cancellationToken)
        {
            await themeService.ApplyAsync(cancellationToken);

            StatusMessage = "Тема применена";
        }

        /// <summary>
        /// Сохраняет текущий черновик как пользовательскую тему.
        /// </summary>
        [RelayCommand]
        private async Task SaveDraftAsync(CancellationToken cancellationToken)
        {
            await themeService.SaveDraftAsCustomAsync(name: null, cancellationToken);
            await ReloadTreesAsync(cancellationToken);

            StatusMessage = "Черновик сохранен";
        }

        /// <summary>
        /// Сохраняет и применяет текущий черновик.
        /// </summary>
        [RelayCommand]
        private async Task SaveAndApplyDraftAsync(CancellationToken cancellationToken)
        {
            await themeService.SaveDraftAsCustomAsync(name: null, cancellationToken);
            await themeService.ApplyAsync(cancellationToken);
            await ReloadTreesAsync(cancellationToken);

            StatusMessage = "Тема сохранена и применена";
        }

        /// <summary>
        /// Сбрасывает черновик к текущей примененной теме.
        /// </summary>
        [RelayCommand]
        private async Task ResetDraftAsync(CancellationToken cancellationToken)
        {
            await themeService.ResetDraftAsync(cancellationToken);

            StatusMessage = "Черновик сброшен";
        }

        /// <summary>
        /// Удаляет тему из хранилища.
        /// </summary>
        [RelayCommand]
        private async Task DeleteThemeAsync(ThemeDefinition? theme, CancellationToken cancellationToken)
        {
            if (theme is null) return;

            if (theme.IsBuiltIn)
            {
                StatusMessage = "Встроенную тему нельзя удалить. Сначала добавьте её в пресеты.";
                return;
            }

            await themeRepository.DeleteAsync(theme.Id, cancellationToken);
            await ReloadTreesAsync(cancellationToken);

            StatusMessage = $"Тема удалена: {theme.Name}";
        }

        /// <summary>
        /// Добавляет/обновляет тему в избранных пресетах.
        /// </summary>
        [RelayCommand]
        private async Task FavoriteThemeAsync(ThemeDefinition? theme, CancellationToken cancellationToken)
        {
            if (theme is null) return;

            ThemeDefinition updated = theme with
            {
                IsFavorite = true,
                UpdatedUtc = DateTimeOffset.UtcNow
            };

            await themeRepository.SaveAsync(updated, cancellationToken);
            await ReloadTreesAsync(cancellationToken);

            StatusMessage = $"Тема добавлена в пресеты: {updated.Name}";
        }

        private async Task SelectPresetAsync(Guid id)
        {
            try
            {
                await themeService.SelectPresetAsync(id);

                StatusMessage = "Тема выбрана для предпросмотра";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка выбора темы: {ex.Message}";
            }
        }

        private async Task AddThemeAsPresetAsync(ThemeDefinition theme, CancellationToken cancellationToken)
        {
            await themeService.UpdateDraftAsync(theme, cancellationToken);
            await themeService.SaveDraftAsCustomAsync(theme.Name, cancellationToken);
            await ReloadTreesAsync(cancellationToken);

            StatusMessage = $"Тема добавлена в пресеты: {theme.Name}";
        }

        private void ActivateSettingGroup(ThemeTreeNodeViewModel node)
        {
            isGlobalEditor = node.Descriptor is null || node.Payload as string == "Global";
            selectedControlType = node.Descriptor?.ControlType;
            selectedState = node.State;
            selectedGroup = node.Group;
            selectedDescriptor = node.Descriptor;

            ActiveEditor = node.Group switch
            {
                ThemeSettingGroup.Typography => EditorKind.Typography,
                ThemeSettingGroup.Geometry => EditorKind.Geometry,
                ThemeSettingGroup.Colors => EditorKind.ComponentColors,
                ThemeSettingGroup.Effects => EditorKind.Effect,
                _ => EditorKind.None
            };

            LoadEditorsFromDraft();
        }

        private async Task ReloadTreesAsync(CancellationToken cancellationToken)
        {
            IReadOnlyList<ThemeDefinition> presets = await themeService.GetPresetsAsync(cancellationToken);

            ThemeTreeNodeViewModel readyRoot = BuildReadyRoot(presets);
            ReadyTree.SetRoots([readyRoot]);

            ThemeTreeNodeViewModel customRoot = ThemeTreeBuilder.BuildCustomRoot(controlCatalog);
            CustomTree.SetRoots([customRoot]);

            OnPropertyChanged(nameof(HasUnsavedChanges));
        }

        private ThemeTreeNodeViewModel BuildReadyRoot(IEnumerable<ThemeDefinition> themes)
        {
            ThemeTreeNodeViewModel root = ThemeTreeBuilder.BuildReadyThemesRoot(themes);

            ThemeTreeNodeViewModel settings = new ()
            {
                Title = "Настройки интерфейса",
                Kind = TreeNodeKind.Category,
                IsExpanded = true
            };

            settings.Children.Add(new ThemeTreeNodeViewModel
            {
                Title = "Типографика",
                Detail = "Глобальные настройки текста",
                Kind = TreeNodeKind.SettingGroup,
                Group = ThemeSettingGroup.Typography,
                Payload = "Global",
                IsExpanded = false
            });

            settings.Children.Add(new ThemeTreeNodeViewModel
            {
                Title = "Геометрия",
                Detail = "Глобальные отступы и скругления",
                Kind = TreeNodeKind.SettingGroup,
                Group = ThemeSettingGroup.Geometry,
                Payload = "Global",
                IsExpanded = false
            });

            settings.Children.Add(new ThemeTreeNodeViewModel
            {
                Title = "Эффекты",
                Detail = "Глобальные визуальные эффекты",
                Kind = TreeNodeKind.SettingGroup,
                Group = ThemeSettingGroup.Effects,
                Payload = "Global",
                IsExpanded = false
            });

            root.Children.Insert(0, settings);

            ThemeTreeNodeViewModel actions = new ()
            {
                Title = "Действия",
                Kind = TreeNodeKind.Category,
                IsExpanded = true
            };

            actions.Children.Add(new ThemeTreeNodeViewModel
            {
                Title = "Создать простую тему",
                Kind = TreeNodeKind.Action,
                Payload = EditorKind.AddSolid
            });

            actions.Children.Add(new ThemeTreeNodeViewModel
            {
                Title = "Создать градиентную тему",
                Kind = TreeNodeKind.Action,
                Payload = EditorKind.AddGradient
            });

            root.Children.Add(actions);

            return root;
        }

        private void LoadEditorsFromDraft()
        {
            if (isLoadingFromDraft) return;

            isLoadingFromDraft = true;

            try
            {
                ThemeDefinition draftTheme = themeService.Draft;
                Draft = draftTheme;

                switch (ActiveEditor)
                {
                    case EditorKind.Typography:
                        if (isGlobalEditor)
                        {
                            TypographyEditor.LoadFrom(draftTheme.GlobalTypography);
                        }
                        else if (!string.IsNullOrWhiteSpace(selectedControlType))
                        {
                            ComponentTheme component = draftTheme.GetComponentTheme(selectedControlType, selectedState);
                            TypographyEditor.LoadFrom(component.Typography);
                        }
                        break;
                    case EditorKind.Geometry:
                        if (isGlobalEditor)
                        {
                            GeometryEditor.LoadFrom(draftTheme.GlobalGeometry);
                        }
                        else if (!string.IsNullOrWhiteSpace(selectedControlType))
                        {
                            ComponentTheme component = draftTheme.GetComponentTheme(selectedControlType, selectedState);
                            GeometryEditor.LoadFrom(component.Geometry);
                        }
                        break;
                    case EditorKind.ComponentColors:
                        if (!string.IsNullOrWhiteSpace(selectedControlType))
                        {
                            ComponentEditor.LoadFrom(draftTheme, selectedControlType, selectedState, selectedDescriptor);
                        }
                        break;
                    case EditorKind.Effect:
                        if (isGlobalEditor)
                        {
                            EffectEditor.LoadFrom(draftTheme.GlobalEffect);
                        }
                        else if (!string.IsNullOrWhiteSpace(selectedControlType))
                        {
                            ComponentTheme component = draftTheme.GetComponentTheme(selectedControlType, selectedState);
                            EffectEditor.LoadFrom(component.Effects);
                        }
                        break;
                }

                OnPropertyChanged(nameof(HasUnsavedChanges));
            }
            finally
            {
                isLoadingFromDraft = false;
            }
        }

        private async void OnEditorPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (isLoadingFromDraft || isUpdatingFromEditor) return;

            try
            {
                isUpdatingFromEditor = true;

                await UpdateDraftFromActiveEditorAsync(e.PropertyName);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка обновления черновика: {ex.Message}";
            }
            finally
            {
                isUpdatingFromEditor = false;
            }
        }

        private async Task UpdateDraftFromActiveEditorAsync(string? propertyName)
        {
            ThemeDefinition draftTheme = themeService.Draft;
            ThemeDefinition? updated = null;

            switch (ActiveEditor)
            {
                case EditorKind.AddSolid:
                    if (propertyName is nameof(SolidThemeEditorViewModel.Harmony) or nameof(SolidThemeEditorViewModel.BaseHex))
                    {
                        await SolidEditor.GenerateCommand.ExecuteAsync(null);
                    }

                    if (SolidEditor.PreviewTheme is not null)
                    {
                        updated = SolidEditor.PreviewTheme;
                    }
                    break;
                case EditorKind.AddGradient:
                    if (IsGradientEditorProperty(propertyName))
                    {
                        await GradientEditor.BuildCommand.ExecuteAsync(null);
                    }

                    if (GradientEditor.PreviewTheme is not null)
                    {
                        updated = GradientEditor.PreviewTheme;
                    }
                    break;
                case EditorKind.Typography:
                    TypographySettings typography = TypographyEditor.BuildSettings();

                    updated = isGlobalEditor
                        ? draftTheme with { GlobalTypography = typography, UpdatedUtc = DateTimeOffset.UtcNow }
                        : draftTheme.WithComponentTypography(selectedControlType!, selectedState, typography);
                    break;
                case EditorKind.Geometry:
                    GeometrySettings geometry = GeometryEditor.BuildSettings();

                    updated = isGlobalEditor
                        ? draftTheme with { GlobalGeometry = geometry, UpdatedUtc = DateTimeOffset.UtcNow }
                        : draftTheme.WithComponentGeometry(selectedControlType!, selectedState, geometry);
                    break;
                case EditorKind.ComponentColors:
                    if (!string.IsNullOrWhiteSpace(selectedControlType))
                    {
                        ComponentTheme component = ComponentEditor.BuildComponentTheme(draftTheme);

                        updated = draftTheme.WithComponent(selectedControlType, selectedState, component);
                    }
                    break;
                case EditorKind.Effect:
                    EffectSettings effect = EffectEditor.BuildEffectSettings();

                    if (isGlobalEditor)
                    {
                        updated = draftTheme with
                        {
                            GlobalEffect = effect,
                            UpdatedUtc = DateTimeOffset.UtcNow
                        };
                    }
                    else if (!string.IsNullOrWhiteSpace(selectedControlType))
                    {
                        updated = draftTheme.WithComponentEffects(selectedControlType, selectedState, effect);
                    }
                    break;
            }

            if (updated is not null)
            {
                await themeService.UpdateDraftAsync(updated);
                Draft = updated;
                OnPropertyChanged(nameof(HasUnsavedChanges));
            }
        }

        private static bool IsGradientEditorProperty(string? propertyName)
        {
            return propertyName is null
                or nameof(GradientThemeEditorViewModel.Settings)
                or nameof(GradientThemeEditorViewModel.PreviewTheme);
        }

        private async void OnThemeServiceDraftChanged(object? sender, ThemeChangedEventArgs e)
        {
            if (isUpdatingFromEditor) return;

            Draft = e.Theme;

            LoadEditorsFromDraft();
            await MainThread.InvokeOnMainThreadAsync(() => OnPropertyChanged(nameof(HasUnsavedChanges)));
        }

        private void OnThemeServiceApplied(object? sender, ThemeChangedEventArgs e)
        {
            Draft = e.Theme;
            StatusMessage = e.IsApplied ? $"Применена тема: {e.Theme.Name}" : "Черновик обновлен";

            OnPropertyChanged(nameof(HasUnsavedChanges));
        }

        private async void OnThemeRepositoryChanged(object? sender, EventArgs e) => await ReloadTreesAsync(CancellationToken.None);
    }
}
