using SE_WPF.Model;
using SE_WPF.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE_WPF.Presenter
{
    /// <summary>
    /// Presenter layer
    /// </summary>
    class MyPresenter
    {
        private IModel m_model;
        private IView m_view;
        private string m_pathToIndexOnDisk;
        private List<string> m_chosenLangs;
        public MyPresenter(IModel model, IView view)
        {
            m_model = model;
            m_view = view;
            m_pathToIndexOnDisk = "";
            SetEvents();
        }

        private void SetEvents()
        {
            m_model.ModelChanged += Mperform;
            m_view.ViewChanged += PerformCommand;
            
        }

        private void PerformCommand(string command)
        {
            if(command == "saveQResults")
            {
                bool savePressed = false;
                string saveParams = m_view.getSaveQueryParams(ref savePressed);
                if (!savePressed)
                {
                    
                    return;
                }
                string[] splited = saveParams.Split(' ');
                string path = splited[0];
                string what = splited[1];
                if(path == "x" && what == "x")
                {
                    m_view.Output("You must chose a path and what to save");
                    return;
                }
                if (path == "x" && what != "x")
                {
                    m_view.Output("You must chose a path ");
                    return;
                }
                if (path != "x" && what == "x")
                {
                    m_view.Output("You must chose what to save");
                    return;
                }

                
                m_model.saveQueryResults(path, what); 
            }
            if(command == "singleQuery")
            {
                bool searchPressed = false;
                string query = m_view.getSingleQuery(ref searchPressed);
                if (!searchPressed)
                {
                    return;
                }
                if(query == "" || query == null || query == string.Empty)
                {
                    m_view.Output("You must type a query :( ");
                    return;
                }
                List<string> chosenLangs = m_chosenLangs;
                query = query.ToUpper();
                m_model.performSearch(query, chosenLangs);
                m_chosenLangs = null;

            }
            if(command == "fileQuery")
            {
                bool searchPressed = false;
                string pathToQuriesFile = m_view.getQuriesFilePath(ref searchPressed);
                if (!searchPressed)
                {
                    return;
                }
                if(pathToQuriesFile == "x")
                {
                    m_view.Output("You must enter a path :( ");
                    return;
                }
                m_model.performFileSearch(pathToQuriesFile,m_chosenLangs);
                m_chosenLangs = null;
            }
            
            if(command == "Generate Index")
            {
                string IndexParameters = m_view.getIndexParameters();
                if(IndexParameters == "x|x|x|x" || IndexParameters == "x|x|False|x")
                {
                    m_view.Output("All fields needs to be filled\n\n come on :(");
                    return;
                }
                if (IndexParameters == "c|c|c|c" || IndexParameters == "c|c|False|c")
                {
                   
                    return;
                }
                string[] IndParams = IndexParameters.Split('|');
                string corpusPath = IndParams[0];
                string stopWordsPath = IndParams[1];
                string stemOn = IndParams[2];

                bool isStemOn=false;
                if(stemOn == "True")
                {
                    isStemOn = true;
                }
                if (stemOn == "False")
                {
                    isStemOn = false;
                }
                string saveIndOn = IndParams[3];
                m_pathToIndexOnDisk = saveIndOn;
                m_model.GenerateIndex(corpusPath, stopWordsPath,isStemOn, saveIndOn);
            }

            if(command == "Display")
            {
                Dictionary<string, long> dictionary = m_model.GetDictionary();
                if(dictionary == null)
                {
                    m_view.Output("Dictionary hasn't been generated yet");
                    return;
                }
                m_view.outputDic(dictionary);

            }
            if(command == "Reset")
            {
                Dictionary<string,long> dict = m_model.GetRealDictionary();
                if (dict == null || dict.Count == 0)
                {
                    m_view.Output("There is nothing to reset yet :)");
                    return;
                }
                m_model.reset(m_pathToIndexOnDisk);
                m_view.Output("Program Reset Acction Succeeded :)\n\nThere is no turning back the clock!");
            }

            if(command == "SaveDic")
            {
                string pathToSaveDicOn = m_view.getPathToSaveDic();
                if (pathToSaveDicOn == "x")
                    return;
                m_model.saveDicOnDisk(pathToSaveDicOn);
            }
            if(command == "language")
            {
                HashSet<string> lang = m_model.getLangs(); 
                if(lang == null || lang.Count == 0)
                {
                    m_view.Output("Index hasn't been created yet, so no languages for you :(");
                    return;
                }
                m_chosenLangs = m_view.getChosenLangs(lang);
            }
            if(command == "loadDict")
            {
                bool isLoadSelected =false;
                string pathToRelevantFiles = m_view.getDictPath(ref isLoadSelected);
                if (!isLoadSelected)
                {
                    return;
                }
                //take from view stem/off and path to model.load(...)
                string[] splited = pathToRelevantFiles.Split(' ');
                string dict = splited[0];
                string docsLength = splited[1];
                string N = splited[2];
                string posting = splited[3];
                string docLangDict = splited[4];
                string langs = splited[5];
                string dicTF = splited[6];
                m_model.LoadDictionary(dict, docsLength, N, posting, docLangDict,langs, dicTF);
            }
        }

        private void Mperform(string problemCode)
        {
            if(problemCode == "searchResultsReady")
            {
                m_view.outputSearchresult(m_model.getSearchResults());
            }
           else if (problemCode == "fileSearchResultsReady")
            {
                m_view.outputFileSearchResult(m_model.getFileSearchRes());
            }
            else 
            m_view.Output(problemCode);
        }
    }
}
