﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using MediaManager.Forms;
using MediaManager.Helpers;
using MediaManager.ViewModel;

namespace MediaManager.Commands
{
    public class PreferenciasCommands
    {
        public class CommandLimparBancoDeDados : ICommand
        {
            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                if (MessageBox.Show("Ao realizar esta ação, todas as informações armazenadas sobre o conteúdo que você possui serão apagadas (entretanto todos os arquivos de vídeo que você possui permanecerá intacto). Você realmente deseja realizar esta ação?", Properties.Settings.Default.AppName, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    using (Model.Context db = new Model.Context())
                    {
                        db.Database.ExecuteSqlCommand(@"/*Disable Constraints & Triggers*/
exec sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'
exec sp_MSforeachtable 'ALTER TABLE ? DISABLE TRIGGER ALL'

/*Perform delete operation on all table for cleanup*/
exec sp_MSforeachtable 'DELETE ?'

/*Enable Constraints & Triggers again*/
exec sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL'
exec sp_MSforeachtable 'ALTER TABLE ? ENABLE TRIGGER ALL'

/*Reset Identity on tables with identity column*/
exec sp_MSforeachtable 'IF OBJECTPROPERTY(OBJECT_ID(''?''), ''TableHasIdentity'') = 1 BEGIN DBCC CHECKIDENT (''?'',RESEED,0) END'");
                    }
                    frmMain.MainVM.AtualizarConteudo(Enums.TipoConteudo.AnimeFilmeSérie);
                }
            }
        }

        public class CommandEscolherPastaAnimes : ICommand
        {
            public event EventHandler CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                if (parameter is PreferenciasViewModel)
                {
                    PreferenciasViewModel preferenciasVM = parameter as PreferenciasViewModel;
                    Ookii.Dialogs.VistaFolderBrowserDialog folderDialog = new Ookii.Dialogs.VistaFolderBrowserDialog();
                    folderDialog.SelectedPath = preferenciasVM.sPastaAnimes;
                    if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        preferenciasVM.sPastaAnimes = folderDialog.SelectedPath;
                    }
                }
            }
        }

        public class CommandEscolherPastaFilmes : ICommand
        {
            public event EventHandler CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                if (parameter is PreferenciasViewModel)
                {
                    PreferenciasViewModel preferenciasVM = parameter as PreferenciasViewModel;
                    Ookii.Dialogs.VistaFolderBrowserDialog folderDialog = new Ookii.Dialogs.VistaFolderBrowserDialog();
                    folderDialog.SelectedPath = preferenciasVM.sPastaFilmes;
                    if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        preferenciasVM.sPastaFilmes = folderDialog.SelectedPath;
                    }
                }
            }
        }

        public class CommandEscolherPastaSeries : ICommand
        {
            public event EventHandler CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                if (parameter is PreferenciasViewModel)
                {
                    PreferenciasViewModel preferenciasVM = parameter as PreferenciasViewModel;
                    Ookii.Dialogs.VistaFolderBrowserDialog folderDialog = new Ookii.Dialogs.VistaFolderBrowserDialog();
                    folderDialog.SelectedPath = preferenciasVM.sPastaSeries;
                    if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        preferenciasVM.sPastaSeries = folderDialog.SelectedPath;
                    }
                }
            }
        }

        public class CommandEscolherPastaDownloads : ICommand
        {
            public event EventHandler CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                if (parameter is PreferenciasViewModel)
                {
                    PreferenciasViewModel preferenciasVM = parameter as PreferenciasViewModel;
                    Ookii.Dialogs.VistaFolderBrowserDialog folderDialog = new Ookii.Dialogs.VistaFolderBrowserDialog();
                    folderDialog.SelectedPath = preferenciasVM.sPastaDownloads;
                    if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        preferenciasVM.sPastaDownloads = folderDialog.SelectedPath;
                    }
                }
            }
        }

        public class CommandSalvar : ICommand
        {
            public event EventHandler CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                if (parameter is PreferenciasViewModel)
                {
                    PreferenciasViewModel preferenciasVM = parameter as PreferenciasViewModel;
                    DBHelper DBHelper = new DBHelper();

                    Properties.Settings.Default.pref_FormatoAnimes = !string.IsNullOrWhiteSpace(preferenciasVM.sFormatoParaAnimes) ? preferenciasVM.sFormatoParaAnimes : "{Titulo} - {Absoluto} - {TituloEpisodio}";
                    Properties.Settings.Default.pref_FormatoFilmes = !string.IsNullOrWhiteSpace(preferenciasVM.sFormatoParaFilmes) ? preferenciasVM.sFormatoParaFilmes : "{Titulo} ({Ano})";
                    Properties.Settings.Default.pref_FormatoSeries = !string.IsNullOrWhiteSpace(preferenciasVM.sFormatoParaSeries) ? preferenciasVM.sFormatoParaSeries : "{Titulo} - {SxEE} - {TituloEpisodio}";
                    Properties.Settings.Default.pref_IdiomaPesquisa = preferenciasVM.sIdiomaSelecionado;
                    Properties.Settings.Default.pref_IntervaloDeProcuraConteudoNovo = preferenciasVM.nIntervaloDeProcuraConteudoNovo;
                    Properties.Settings.Default.pref_PastaDownloads = preferenciasVM.sPastaDownloads;
                    Properties.Settings.Default.pref_MetodoDeProcessamento = (int)preferenciasVM.nIdMetodoDeProcessamentoSelecionado;

                    List<string> alterados = new List<string>();
                    bool bFlAlterarPastas = false;

                    if (Properties.Settings.Default.pref_PastaFilmes != preferenciasVM.sPastaFilmes)
                    {
                        alterados.Add("filmes");
                    }

                    if (Properties.Settings.Default.pref_PastaSeries != preferenciasVM.sPastaSeries)
                    {
                        alterados.Add("séries");
                    }

                    if (Properties.Settings.Default.pref_PastaAnimes != preferenciasVM.sPastaAnimes)
                    {
                        alterados.Add("animes");
                    }

                    Properties.Settings.Default.pref_PastaAnimes = preferenciasVM.sPastaAnimes;
                    Properties.Settings.Default.pref_PastaFilmes = preferenciasVM.sPastaFilmes;
                    Properties.Settings.Default.pref_PastaSeries = preferenciasVM.sPastaSeries;

                    Properties.Settings.Default.Save();

                    if (alterados.Count > 0 && Helper.MostrarMensagem("Deseja alterar também as pastas dos(as) " + Helper.ColocarVirgula(null, alterados) + " já cadastrados(as)?", Enums.eTipoMensagem.QuestionamentoSimNao)
                            == MessageBoxResult.Yes)
                    {
                        bFlAlterarPastas = true;
                    }

                    if (bFlAlterarPastas)
                    {
                        if (alterados.Contains("animes"))
                        {
                            DBHelper.AlterarPastaPadraoVideos(Enums.TipoConteudo.Anime, preferenciasVM.sPastaAnimes);
                        }
                        if (alterados.Contains("filmes"))
                        {
                            DBHelper.AlterarPastaPadraoVideos(Enums.TipoConteudo.Filme, preferenciasVM.sPastaFilmes);
                        }
                        if (alterados.Contains("séries"))
                        {
                            DBHelper.AlterarPastaPadraoVideos(Enums.TipoConteudo.Série, preferenciasVM.sPastaSeries);
                        }

                        frmMain.MainVM.AtualizarConteudo(Enums.TipoConteudo.AnimeFilmeSérie);
                    }

                    if (preferenciasVM.ActionFechar != null)
                    {
                        preferenciasVM.ActionFechar();
                    }
                }
            }
        }
    }
}