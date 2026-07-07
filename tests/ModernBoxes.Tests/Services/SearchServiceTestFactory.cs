using ModernBoxes.Core.Interfaces.Repositories;
using ModernBoxes.Infrastructure.Data;
using ModernBoxes.Infrastructure.Query;
using ModernBoxes.Infrastructure.Services;
using ModernBoxes.Sdk.Search;
using Moq;

namespace ModernBoxes.Tests.Services;

internal static class SearchServiceTestFactory
{
    public static SearchService Create(
        IMenuRepository menu,
        IApplicationRepository app,
        ITempDirRepository dir,
        ITempFileRepository file,
        INoteRepository note)
    {
        var frecency = new FrecencyService(DatabaseService.Instance);
        var plugins = new ISearchPlugin[]
        {
            new MenuSearchPlugin(menu),
            new ApplicationSearchPlugin(app),
            new TempDirSearchPlugin(dir),
            new TempFileSearchPlugin(file),
            new NoteSearchPlugin(note),
        };
        var registry = new SearchPluginRegistry();
        registry.Replace(plugins);
        var engine = new QueryEngine(registry, frecency);
        return new SearchService(engine);
    }

    public static SearchService Create(
        Mock<IMenuRepository> menu,
        Mock<IApplicationRepository> app,
        Mock<ITempDirRepository> dir,
        Mock<ITempFileRepository> file,
        Mock<INoteRepository> note) =>
        Create(menu.Object, app.Object, dir.Object, file.Object, note.Object);
}
