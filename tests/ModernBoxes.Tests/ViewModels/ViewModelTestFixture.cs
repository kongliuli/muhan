using ModernBoxes.Core.Interfaces;
using ModernBoxes.Cards;
using Moq;

namespace ModernBoxes.Tests.ViewModels;

public class ViewModelTestFixture
{
    public Mock<INoteCardService> NoteCardServiceMock { get; } = new();
    public Mock<IApplicationCardService> ApplicationCardServiceMock { get; } = new();
    public Mock<IDirectoryCardService> DirectoryCardServiceMock { get; } = new();
    public Mock<IFileCardService> FileCardServiceMock { get; } = new();

    public NoteCardViewModel CreateNoteCardViewModel() =>
        new(NoteCardServiceMock.Object);

    public ApplicationCardViewModel CreateApplicationCardViewModel() =>
        new(ApplicationCardServiceMock.Object);

    public DirectoryCardViewModel CreateDirectoryCardViewModel() =>
        new(DirectoryCardServiceMock.Object);

    public FileCardViewModel CreateFileCardViewModel() =>
        new(FileCardServiceMock.Object);
}
