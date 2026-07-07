using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ModernBoxes.Core.Models
{
    public class NoteModel : ObservableObject
    {
        private Guid id = Guid.NewGuid();

        public Guid Id
        {
            get { return id; }
            set { id = value; OnPropertyChanged("Id"); }
        }

        private String title = String.Empty;

        public String Title
        {
            get { return title; }
            set { title = value; OnPropertyChanged("Title"); }
        }

        private String content = String.Empty;

        public String Content
        {
            get { return content; }
            set { content = value; OnPropertyChanged("Content"); }
        }

        private String color = "#FFE4B5";

        public String Color
        {
            get { return color; }
            set { color = value; OnPropertyChanged("Color"); }
        }

        private Boolean isPinned;

        public Boolean IsPinned
        {
            get { return isPinned; }
            set { isPinned = value; OnPropertyChanged("IsPinned"); }
        }

        private DateTime createdAt = DateTime.Now;

        public DateTime CreatedAt
        {
            get { return createdAt; }
            set { createdAt = value; OnPropertyChanged("CreatedAt"); }
        }

        private DateTime updatedAt = DateTime.Now;

        public DateTime UpdatedAt
        {
            get { return updatedAt; }
            set { updatedAt = value; OnPropertyChanged("UpdatedAt"); }
        }
    }
}
