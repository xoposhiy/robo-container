# Основные принципы конфигурирования #

Основная задача, которую приходится решать контейнеру — это определение того, какие реализации (pluggables) подходят для запрошенного сервиса (plugin).

Есть два независимых подхода к решению этой задачи:
  1. Явное конфигурирование. В этом случае вы сами указываете какие типы или объекты нужно использовать в качестве реализаций для данного сервиса.
  1. Автоматика. В этом случае контенер сканирует доступные ему сборки на предмет тех типов, которые могли бы выступать в роли реализации для запрошенного сервиса. В результате возвращаются объекты всех подошедших типов.

Предполагается, что эти два способа будут активно комбинироваться: оставлять работу автоматики, вмешиваясь явным конфигурированием лишь там, где умолчания автоматики не подходят.

У контейнера есть ещё одна работа помимо выбора подходящих pluggables. Если у контейнера два раза запросили один и тот же plugin, то должен ли контейнер вернуть один и тот же объект или он должен создавать новый объект на каждый запрос? Поведение можно выбрать самостоятельно, указав политику повторного использования для плагина или pluggable.

Все конфигурирование можно разделить на конфигурирование на уровне сервиса (plugin) и на конфигурирование на уровне реализации (pluggable).

На каждом уровне конфигурирование можно производить с помощью атрибутов, а можно с помощью динамического конфигурирования и конструктора контейнера.

Основным способом считается динамическое конфигурирование и оно всегда перекрывает (или дополняет — в зависимости от смысла конфигурируемого параметра) конфигурирование с помощью атрибутов.

С другой стороны, если конфигурирование реализации (pluggable)  противоречит конфигурированию соответствующего сервиса (plugin), то конфигурирование реализации перекрывает конфигурирование сервиса.

Ниже перечислены уровни конфигурирования в порядке возрастания приоритетности:

  * атрибуты pluggable _(низкий приоритет)_;
  * динамическое конфигурирование pluggable;
  * атрибуты plugin;
  * динамическое конфигурирование plugin _(высокий приоритет)_.



## Интерфейс динамического конфигурирования ##

Все конфигурирование контейнера можно провести с помощью конструктора контейнера.
<a href='Hidden comment: [Configuration.FirstSample'></a>
```

var container = new Container(
	(IContainerConfigurator c) =>
		{
			c.ForPlugin<IPlugin>().UsePluggable<Pluggable>();
			c.ForPluggable<Pluggable>().ReuseIt(ReusePolicy.Never);
			c.ScanLoadedAssemblies(assembly => (assembly.FullName + "").Contains("RoboContainer"));
			c.Logging.Disable();
		});
```
<a href='Hidden comment: '></a>
В этом примере `c` — это объект специального интерфейса, через который можно конфигурировать контейнер. Что происходит в этом примере?

Сначала конфигурируется plugin.
Следующей строкой конфигурируется pluggable.
Затем типы, с которыми будет работать контейнер ограничиваются только сборками `RoboContainer`'а. И, наконец, отключается логгирование.

Что ещё можно сконфигурировать? Полный список методов интерфейса `IContainerConfigurator` представлен ниже:

<a href='Hidden comment: [IContainerConfigurator.interface'></a>
```
void RegisterInitializer(IPluggableInitializer[] initializers);
void ScanAssemblies(Assembly[] assembliesToScan);
void ScanAssemblies(IEnumerable<Assembly> assembliesToScan);
void ScanCallingAssembly();
void ScanLoadedAssemblies();
void ScanLoadedAssemblies(Func<Assembly, Boolean> shouldScan);
void ScanTypesWith(ScannerDelegate scanner);
void ScanLoadedCompanyAssemblies();
void ScanLoadedAssembliesWithPrefix(String companyPrefix);
IPluggableConfigurator ForPluggable(Type pluggableType);
IPluggableConfigurator<TPluggable> ForPluggable<TPluggable>();
IPluginConfigurator<TPlugin> ForPlugin<TPlugin>();
IPluginConfigurator ForPlugin(Type pluginType);
void ForceInjectionOf<TPlugin>(ContractRequirement[] requiredContracts);
IExternalConfigurator ConfigureBy { get; }
ILoggingConfigurator Logging { get; }

```
<a href='Hidden comment: '></a>

## Конфигурирование на уровне сервиса ##

Результат такого конфигурирования действует сразу на все реализации, которые могут быть возвращены при запросе заданного сервиса.
Такое конфигурирование можно произвести при помощи метода `IContainerConfigurator.ForPlugin`. Он возвращает следующий интерфейс:
<a href='Hidden comment: [IGenericPluginConfigurator.interface'></a>
```
IPluginConfigurator<TPlugin> UsePluggable<TPluggable>(ContractDeclaration[] declaredContracts);
IPluginConfigurator<TPlugin> UsePluggable(Type pluggableType, ContractDeclaration[] declaredContracts);
IPluginConfigurator<TPlugin> UseInstance(TPlugin instance, ContractDeclaration[] declaredContracts);
IPluginConfigurator<TPlugin> UseInstanceCreatedBy(CreatePluggableDelegate<TPlugin> createPluggable, ContractDeclaration[] declaredContracts);
IPluginConfigurator<TPlugin> RequireContracts(ContractRequirement[] requiredContracts);
IPluginConfigurator<TPlugin> DontUse<TPluggable>();
IPluginConfigurator<TPlugin> DontUse(Type[] pluggableTypes);
IPluginConfigurator<TPlugin> UsePluggablesAutosearch(Boolean useAutosearch);
IPluginConfigurator<TPlugin> ReusePluggable(ReusePolicy reusePolicy);
IPluginConfigurator<TPlugin> ReusePluggable(IReusePolicy reusePolicy);
IPluginConfigurator<TPlugin> SetInitializer(InitializePluggableDelegate<TPlugin> initializePluggable);
IPluginConfigurator<TPlugin> SetInitializer(Action<TPlugin> initializePlugin);

```
<a href='Hidden comment: '></a>

Для поиска реализаций запрошенного сервиса, у контейнера есть два механизма: в первую очередь он смотрит на типы, явно указанные при конфигурировании как реализации данного сервиса; кроме того у контейнера есть механизм автоматического поиска реализаций. Автоматика отключается, как только вы берете управление на себя и указываете хотя бы одну реализацию явно. Однако автоматику всегда можно явно включить:

<a href='Hidden comment: [Configuration.Plugin'></a>
```

var explicitlySpecifiedPluggable = new Pluggable();
var container = new Container(
	c =>
	c.ForPlugin<IPlugin>()
		.UseInstance(explicitlySpecifiedPluggable) //автоматика отключилась
		.UsePluggablesAutosearch(true)  //но мы ее принудительно включили обратно
		.ReusePluggable(ReusePolicy.Never)
	);
IEnumerable<IPlugin> pluggables = container.GetAll<IPlugin>();
CollectionAssert.Contains(pluggables, explicitlySpecifiedPluggable);
Assert.AreEqual(2, pluggables.Count()); // вторую реализацию нашла автоматика.
```
<a href='Hidden comment: '></a>

## Конфигурирование на уровне реализации ##

Для каждого конкретного типа, который мог бы выступать в роли реализации (pluggable), можно сконфигурировать то, как он будет создаваться,
инициализироваться, какие для него будут определены контракты и нужно ли рассматривать этот тип при автоматическом поиске реализаций.

Все это можно сделать с помощью метода `IContainerConfigurator.ForPlugin`, который возвращает следующий интерфейс:
<a href='Hidden comment: [IGenericPluggableConfigurator.interface'></a>
```
IPluggableConfigurator<TPluggable> DeclareContracts(ContractDeclaration[] contractsDeclaration);
IPluggableConfigurator<TPluggable> ReuseIt(ReusePolicy reusePolicy);
IPluggableConfigurator<TPluggable> ReuseIt(IReusePolicy reusePolicy);
IPluggableConfigurator<TPluggable> DontUseIt();
IPluggableConfigurator<TPluggable> UseConstructor(Type[] argsTypes);
IPluggableConfigurator<TPluggable> CreateItBy(CreatePluggableDelegate<TPluggable> create);
IPluggableConfigurator<TPluggable> UseInstance(TPluggable instance);
IPluggableConfigurator<TPluggable> SetInitializer(InitializePluggableDelegate<TPluggable> initializePluggable);
IPluggableConfigurator<TPluggable> SetInitializer(Action<TPluggable> initializePluggable);
IDependencyConfigurator Dependency(String dependencyName);
IDependencyConfigurator DependencyOfType<TDependencyType>();
IDependencyConfigurator DependencyOfType(Type dependencyType);

```
<a href='Hidden comment: '></a>


Кроме того, при конфигурировании реализации, можно задать некотоую специфику о необходимых этой реализации зависимостях.
Зависимости — это, например, параметры конструктора. Соответственно, имена зависимостей — это имена этих самых параметров.
Получить конфигуратор зависимости можно с помощью метода `IPluggableConfigurator.Dependency`.
Интерфейс конфигуратора зависимостей выглядит так:

<a href='Hidden comment: [IDependencyConfigurator.interface'></a>
```
IDependencyConfigurator RequireContracts(ContractRequirement[] requiredContracts);
IDependencyConfigurator UseValue(Object o);
IDependencyConfigurator UsePluggable(Type pluggableType);
IDependencyConfigurator UsePluggable<TPluggable>();

```
<a href='Hidden comment: '></a>

С помощью этого конфигуратора можно подсказать контейнеру откуда брать зависимости для данного класса. Без этих подсказок, контейнер будет самостоятельно искать подходящие реализации.