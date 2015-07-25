﻿using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MediaManager.Model
{
    public interface Video : INotifyPropertyChanged
    {
        IList<string> AvailableTranslations { get; set; }
        string Certification { get; set; }
        string FolderPath { get; set; }
        string Generos { get; }
        IList<string> GenresList { get; set; }
        object Homepage { get; set; }
        int ID { get; set; }
        Ids Ids { get; set; }
        Images Images { get; set; }
        string Language { get; set; }
        string MetadataFolder { get; set; }
        string Overview { get; set; }
        double Rating { get; set; }
        int Runtime { get; set; }
        string Title { get; set; }
        string Traducoes { get; }
        object Trailer { get; set; }
        DateTime? UpdatedAt { get; set; }
        int Votes { get; set; }
        int Year { get; set; }
    }
}