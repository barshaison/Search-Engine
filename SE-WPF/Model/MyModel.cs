
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SE_WPF.Model
{
    /// <summary>
    /// Imodel interface implementation
    /// </summary>
    class MyModel : IModel
    {
        public event modelEventDelegate ModelChanged;

        Dictionary<string/*term*/, long/*TF*/> m_DictionaryTF;
        Dictionary<string/*term*/, long/*Ptr*/> m_Dictionary;
        private string m_savePosOn;
        private ReadFile m_readFile;
        private bool m_stemOn;
        private int m_N;
        private int m_avdl;
        private Dictionary<string/*docID*/, int/*doc length*/> m_docsLength;
        private Searcher m_searcher;
        private string m_pathToPosting;
        private List<string> m_searchResults;
        private Dictionary<string/*query id*/, List<string>/*doc*/> m_fileSearchResults;
        private Dictionary<string, string> m_docLangDict;
        private HashSet<string> m_Langs;
        public static int m_queryID = 900;

        // private Dictionary<string/*word*/, int/*row*/> m_wordsMap;
        // private Dictionary<string/*doc*/, int/*col*/> m_docsMap;
        // private DenseMatrix m_Tk;
        // private DenseMatrix m_Sk_invers;
        // private DenseMatrix m_Dk_T;
        /// <summary>
        /// c'tor
        /// </summary>
        public MyModel()
        {
            m_readFile = new ReadFile();
            m_searcher = new Searcher();
            m_stemOn = false;
        }
        /// <summary>
        /// get file search results
        /// </summary>
        /// <returns></returns>
        public Dictionary<string/*query id*/, List<string>/*doc*/> getFileSearchRes()
        {
            return m_fileSearchResults;
        }
        /// <summary>
        /// saves query results on disk
        /// </summary>
        /// <param name="path"></param>
        /// <param name="what"></param>
        public void saveQueryResults(string path, string what)
        {
            if (what == "single")
            {
                if (m_searchResults == null || m_searchResults.Count == 0)
                {
                    ModelChanged("Single query has not been made yet :(");
                    return;
                }
                saveSingle(path);
                ModelChanged("The results of your last single query search were saved successfuly on " + path + " by the name: single_query_results.txt");
            }

            if(what == "file")
            {
                if (m_fileSearchResults == null || m_fileSearchResults.Count == 0)
                {
                    ModelChanged("File query has not been made yet :(");
                    return;
                }
                saveFile(path);
                ModelChanged("The results of your last file quries search were saved successfuly on " + path + " by the name: file_queries_results.txt");
            }
            
        }
        /// <summary>
        /// saves file query results
        /// </summary>
        /// <param name="path"></param>
        private void saveFile(string path)
        {
           
            using (FileStream fileQRes = new FileStream(path + "\\" + "file_queries_results" + ".txt", FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fileQRes))
                {
                    foreach(string queryId in m_fileSearchResults.Keys)
                    {
                        List<string> currQueryRes = m_fileSearchResults[queryId];
                        for (int i = 0; i < currQueryRes.Count - 1; i++)
                        {
                            string docID = currQueryRes[i];
                            string toWrite = queryId + " 0 " + docID + " 1 " + "2.42 mt";
                            sw.WriteLine(toWrite);
                        }
                    }
                    
                }
            }
        }
        /// <summary>
        /// saves single query results
        /// </summary>
        /// <param name="path"></param>
        private void saveSingle(string path)
        {
            
            using (FileStream singleQRes = new FileStream(path + "\\" + "single_query_results" + ".txt", FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(singleQRes))
                {
                    for (int i=0;i<m_searchResults.Count-1;i++)
                    {
                        string docID = m_searchResults[i];
                        string toWrite = m_queryID + " 0 " + docID + " 1 " + "2.42 mt";
                        sw.WriteLine(toWrite);
                    }
                }
            }
        }
        /// <summary>
        /// get search results
        /// </summary>
        /// <returns></returns>
        public List<string> getSearchResults()
        {
            return m_searchResults;
        }
        /// <summary>
        /// generates the index
        /// </summary>
        /// <param name="pathToCorpus"></param>
        /// <param name="pathToStopWords"></param>
        /// <param name="isStemOn"></param>
        /// <param name="savePosOn"></param>
        public void GenerateIndex(string pathToCorpus, string pathToStopWords, bool isStemOn,string savePosOn)
        {
            m_savePosOn = savePosOn;
            m_stemOn = isStemOn;
            //ReadFile readFile = new ReadFile();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            m_readFile.Readfiles(pathToCorpus, pathToStopWords, isStemOn, savePosOn);
            stopwatch.Stop();
            string runTime ="Indexing Run Time: " + stopwatch.Elapsed;
            m_DictionaryTF = m_readFile.geDictionaryTF();
            m_Dictionary = m_readFile.geDictionary(); //gets the dic with [term : Ptr] 
            int numOfTermsIndexed = m_readFile.getNumOfTermsIndexed();
            saveLangsOndisk(savePosOn, getLangss());
            saveNumOfDocsInCorpusOnDisk(savePosOn, m_readFile.getNumOfDocsInCorpus()); //save N on disk
            savDocsLengthOnDisk(savePosOn, m_readFile.getDocsLength()); // save dl (docsLength) on disk
            saveDocsLangDictonDisk(savePosOn,getDocLangDict()); // save docs Langs dict
            saveDictionaryTFOnDisk(savePosOn); //asvae dict TF
            ModelChanged("Index was created successfully  :)\n\n" + runTime + "\n\n" + "Number Of Unique Terms In The Corpus: " + numOfTermsIndexed + "\n\nNumber Of Documents Indexed: " + m_readFile.getNumOfDocsInCorpus() );
        }
        /// <summary>
        /// load the TF dictionary
        /// </summary>
        /// <param name="dicTFpath"></param>
        private void LoadDicTF(string dicTFpath)
        {
            m_DictionaryTF = new Dictionary<string, long>();
            using (FileStream dicTF = new FileStream(dicTFpath, FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(dicTF))
                {

                    while (br.BaseStream.Position != br.BaseStream.Length)
                    {
                        string rec = br.ReadString();
                        string[] sRec = rec.Split(' ');
                        string docName = sRec[0];
                        string tf = sRec[1];
                        long lTF = 0;
                        long.TryParse(tf, out lTF);
                        try { m_DictionaryTF.Add(docName, lTF); } catch (Exception) { }
                        
                    }
                }
            }
        }
        /// <summary>
        /// save the TF dict
        /// </summary>
        /// <param name="path"></param>
        private void saveDictionaryTFOnDisk(string path)
        {
            string termRecToWrite = "";
            string nameDict = "DictionaryTF";
            if (m_stemOn)
                nameDict = "Stemed-DictionaryTF";

            using (FileStream DicFile = new FileStream(path + "\\" + nameDict + ".txt", FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(DicFile))
                {
                   foreach(string term in m_DictionaryTF.Keys)
                    {
                        termRecToWrite = term + " " + m_DictionaryTF[term];
                        bw.Write(termRecToWrite);
                    }
                }
            }
        }
        /// <summary>
        /// gets docs langs dict
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> getDocLangDict()
        {
            Dictionary<string, string> ans = new Dictionary<string, string>();
            Dictionary<string, string> tmp = m_readFile.getDocLangDict();
            foreach(string doc in tmp.Keys)
            {
                string cleanLang = tmp[doc].Replace(" ", string.Empty);
                ans.Add(doc, cleanLang);
                
            }
            return ans;
        }


        /* private void createWordsMap() //func for LSI
         {
             m_wordsMap = new Dictionary<string, int>();
             int c = 0; string word;
             for(int i = 10000; i< m_Dictionary.Count; i++)
             {
                 word = m_Dictionary.Keys.ElementAt(i);
                 if(Regex.IsMatch(word, @"^[a-zA-Z]+$"))
                 {
                     m_wordsMap.Add(word, c);
                     c++;
                 }
             }
         }*/
         /// <summary>
         /// finds posting record int the posting file
         /// </summary>
         /// <param name="m_pathToPostingFile"></param>
         /// <param name="ptr"></param>
         /// <returns></returns>
        private string findPostingRec(string m_pathToPostingFile, long ptr) // find and return posting rec of certain word 
        {
            string rec = "";
            using (FileStream posFile = new FileStream(m_pathToPostingFile, FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(posFile))
                {
                    while (br.BaseStream.Position != br.BaseStream.Length)
                    {
                        if (ptr == 0)
                        {
                            rec = br.ReadString();
                            break;
                        }
                        else
                        {
                            br.ReadString();
                            if (ptr == br.BaseStream.Position)
                            {
                                rec = br.ReadString();
                                break;
                            }
                        }
                    }
                }
            }
            return rec;
        }

      /*  public void creatMatrixForLSI()
        {
            int rows = m_wordsMap.Count;
            int cols = m_docsMap.Count;
            double[,] A = new double[rows,cols];

            foreach(string word in m_wordsMap.Keys) //create A matrix
            {
                long ptr = m_Dictionary[word];
                string rec = findPostingRec(@"C:\Users\Daniel\Documents\Visual Studio 2015\Projects\IR\IR\bin\Debug\t\PostingF.txt", ptr);
                List<string> docsOfword = new List<string>();
                string[] sRec = rec.Split(' ');
                for (int i = 0; i < sRec.Length; i++)
                {
                    if (sRec[i].EndsWith("#"))
                    {
                        docsOfword.Add(sRec[i].Substring(0, sRec[i].Length - 1));
                    }
                }
                int Arow = m_wordsMap[word];
                foreach(string doc in docsOfword)
                {
                    int Acol = m_docsMap[doc];
                    A[Arow, Acol] = 1;
                }
            }
            var Amatrix = DenseMatrix.OfArray(A);
            var svd = Amatrix.Svd(true);
            var D_T = svd.VT;
            var T = svd.U;
            var S = svd.W;
            int k = 2;
            var D_Tk = D_T.SubMatrix(0, k, 0, D_T.ColumnCount);
            saveMatrixInFile(D_Tk, "D_Tk");
            var Tk = T.SubMatrix(0, T.RowCount, 0, k);
            saveMatrixInFile(Tk, "Tk");
            var Sk = S.SubMatrix(0, k, 0, k);
            var Sk_inver = Sk.Inverse();
            saveMatrixInFile(Sk_inver, "Sk_inver");
        }*/

       /*private void saveMatrixInFile(MathNet.Numerics.LinearAlgebra.Matrix<double> matrix , string path)
        {
            using (Stream stream = File.Open(path, FileMode.Create))
            {
                var bformatter = new BinaryFormatter();

                bformatter.Serialize(stream, matrix);
            }
        }*/

       /* private MathNet.Numerics.LinearAlgebra.Matrix<double> loadMatrixFromFile(string path)
        {
            MathNet.Numerics.LinearAlgebra.Matrix<double> matrix = null;
            using (Stream stream = File.Open(path, FileMode.Open))
            {
                var bformatter = new BinaryFormatter();

                matrix = (MathNet.Numerics.LinearAlgebra.Matrix<double>)bformatter.Deserialize(stream);
            }
            return matrix;
        }*/

       /* private void createDocsMap() //func for LSI
        {
            m_docsMap = new Dictionary<string, int>();
            int c = 0;
            foreach(string doc in m_docsLength.Keys)
            {
                m_docsMap.Add(doc, c);
                c++;
            }

        }*/
        /// <summary>
        /// loads num of docs in curpus (N)
        /// </summary>
        /// <param name="pathTo_N"></param>
        private void Load_N(string pathTo_N)
        {
            using (FileStream nFile = new FileStream(pathTo_N, FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(nFile))
                {
                    
                    while (br.BaseStream.Position != br.BaseStream.Length)
                    {
                        string n = br.ReadString();
                        Int32.TryParse(n, out m_N);

                    }
                }
            }
        }
        /// <summary>
        /// loads docs length dict
        /// </summary>
        /// <param name="pathTo_docsLength"></param>
        private void Load_DocsLength(string pathTo_docsLength)
        {
            m_docsLength = new Dictionary<string, int>();
            using (FileStream dlFile = new FileStream(pathTo_docsLength, FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(dlFile))
                {

                    while (br.BaseStream.Position != br.BaseStream.Length)
                    {
                        string rec = br.ReadString();
                        string[] sRec = rec.Split(' ');
                        string docName = sRec[0];
                        int length;
                        Int32.TryParse(sRec[1], out length);
                        m_docsLength.Add(docName, length);
                    }
                }
            }


            //compute avdl

            int sumLength = 0; int numOfDocs = 0;
            foreach (string doc in m_docsLength.Keys)
            {
                numOfDocs++;
                sumLength += m_docsLength[doc];
            }
            m_avdl = (sumLength / numOfDocs);
        }

        /// <summary>
        /// loads docs langs dict
        /// </summary>
        /// <param name="pathTo_docLangDict"></param>
        private void Load_DocsLangDict(string pathTo_docLangDict)
        {
            m_docLangDict = new Dictionary<string, string>();
            using (FileStream docsLangFile = new FileStream(pathTo_docLangDict, FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(docsLangFile))
                {
                    while (br.BaseStream.Position != br.BaseStream.Length)
                    {
                        string rec = br.ReadString();
                        string[] sRec = rec.Split(' ');
                        string docName = sRec[0];
                       string docLang = sRec[1];
                        m_docLangDict.Add(docName, docLang);
                    }
                }
            }
        }
        /// <summary>
        /// save docs length dict
        /// </summary>
        /// <param name="path"></param>
        /// <param name="docsLength"></param>
        public void savDocsLengthOnDisk(string path, Dictionary<string/*docID*/, int/*doc length*/> docsLength) //save dl (docs length) on disk
        {
            string docsLengthFileName = "DocsLength";
            if (m_stemOn) docsLengthFileName = "Stemmed-DocsLength";
            using (FileStream docsLengthF = new FileStream(path + "\\" + docsLengthFileName + ".txt", FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(docsLengthF))
                {
                   foreach(string doc in docsLength.Keys)
                    {
                        string toWrite = doc + " " + docsLength[doc];
                        bw.Write(toWrite);
                    }
                }
            }
        }
        /// <summary>
        /// save Docs Lang Dict on Disk
        /// </summary>
        /// <param name="path"></param>
        /// <param name="docLangDict"></param>
        public void saveDocsLangDictonDisk(string path, Dictionary<string,string> docLangDict)
        {
            string docsLangs = "DocLangDict";
            //if (m_stemOn) docsLangs = "Stemmed-DocLangDict";
            using (FileStream docLangDictF = new FileStream(path + "\\" + docsLangs + ".txt", FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(docLangDictF))
                {
                    foreach (string doc in docLangDict.Keys)
                    {
                        string toWrite = doc + " " + docLangDict[doc];
                        bw.Write(toWrite);
                    }
                }
            }
        }
        /// <summary>
        /// save Langs On disk
        /// </summary>
        /// <param name="path"></param>
        /// <param name="langs"></param>
        private void saveLangsOndisk(string path, HashSet<string> langs)
        {
            string docsLangs = "Langs";
            using (FileStream langsF = new FileStream(path + "\\" + docsLangs + ".txt", FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(langsF))
                {
                    foreach (string language in langs)
                    {
                        string toWrite = language;
                        bw.Write(toWrite);
                    }
                }
            }
        }
        /// <summary>
        /// save Num Of Docs In Corpus On Disk
        /// </summary>
        /// <param name="path"></param>
        /// <param name="N"></param>
        private void saveNumOfDocsInCorpusOnDisk(string path, int N) //save N on disk
        {
            using (FileStream Nfile = new FileStream(path + "\\" + "N" + ".txt", FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(Nfile))
                {
                    string sN = N.ToString();
                    bw.Write(sN);
                }
            }
        }
        /// <summary>
        /// Get Dictionary TF
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, long> GetDictionary() // dic [term : totalTF ]
        {

            return m_DictionaryTF;
        }
        /// <summary>
        /// Get Dictionary
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, long> GetRealDictionary() // dic [term : ptr ]
        {

            return m_Dictionary;
        }
        /// <summary>
        /// perform Search
        /// </summary>
        /// <param name="query"></param>
        /// <param name="chosenLangs"></param>
        public void performSearch(string query, List<string/*lang*/> chosenLangs)
        {
            if(m_Dictionary == null)
            {
                ModelChanged("You should load or create an index first :(");
                return;
            }
            if (m_stemOn)
            {
                query = query.ToLower();
                Stemmer stemmer = new Stemmer();
                string[] splited = query.Split(' ');
                for (int i = 0; i< splited.Length;i++)
                {
                    splited[i] = stemmer.stemTerm(splited[i]);
                }
                string stemmedQuery = "";
                for(int i=0; i < splited.Length -1; i++)
                {
                    stemmedQuery = splited[i] + " ";
                }
                stemmedQuery += splited[splited.Length - 1];
                query = stemmedQuery;
                query = query.ToUpper();
            }
            m_searcher.setSearcher(m_N, m_Dictionary, m_pathToPosting, m_docsLength, m_avdl, "stop_words.txt");
           m_searchResults = m_searcher.ansForQuery(query,m_docLangDict,chosenLangs);
            m_searchResults.Add(query + "|" + m_queryID);
            m_queryID++;
            ModelChanged("searchResultsReady");
        }

        /// <summary>
        /// perform File Search
        /// </summary>
        /// <param name="pathToFileQ"></param>
        /// <param name="chosenLangs"></param>
        public void performFileSearch(string pathToFileQ, List<string/*lang*/> chosenLangs)
        {
            if (m_Dictionary == null)
            {
                ModelChanged("You should load or create an index first :(");
                return;
            }
            m_fileSearchResults = new Dictionary<string/*query id*/, List<string>/*docs*/>();
            Dictionary<string/*query id*/, string/*query*/> fileQuries = getFileQuries(pathToFileQ);
            foreach(string queryID in fileQuries.Keys)
            {
                string query = fileQuries[queryID];
                m_searcher.setSearcher(m_N, m_Dictionary, m_pathToPosting, m_docsLength, m_avdl, "stop_words.txt");
                m_searchResults = m_searcher.ansForQuery(query, m_docLangDict, chosenLangs);
                m_searchResults.Add(query);
                m_fileSearchResults.Add(queryID, m_searchResults);
            }
            ModelChanged("fileSearchResultsReady");
        }
        /// <summary>
        /// get File Quries
        /// </summary>
        /// <param name="pathToFileQ"></param>
        /// <returns></returns>
        private Dictionary<string, string> getFileQuries(string pathToFileQ)
        {
            Dictionary<string, string> quries = new Dictionary<string, string>();
            StreamReader file = new StreamReader(pathToFileQ);
            
            string line;
            while ((line = file.ReadLine()) != null)
            {
                string[] splitedLine = line.Split(' ');
                string id = splitedLine[0];
                string query = "";
                for (int i = 1; i < splitedLine.Length; i++)
                {
                    query = query + " " + splitedLine[i];
                }
                if(query != null && query!= "")
                {
                    query = query.Substring(1);

                    if (m_stemOn)
                    {

                        Stemmer stemmer = new Stemmer();
                        string[] splited = query.Split(' ');
                        for (int i = 0; i < splited.Length; i++)
                        {
                            splited[i] = splited[i].ToLower();
                            splited[i] = stemmer.stemTerm(splited[i]);
                        }
                        string stemmedQuery = "";
                        for (int i = 0; i < splited.Length - 1; i++)
                        {
                            stemmedQuery = splited[i] + " ";
                        }
                        stemmedQuery += splited[splited.Length - 1];
                        query = stemmedQuery;
                       
                    }

                    query = query.ToUpper();
                    quries.Add(id, query);
                }
                
            }
            return quries;
        }
        /// <summary>
        /// save Dict On Disk
        /// </summary>
        /// <param name="path"></param>
        public void saveDicOnDisk(string path)
        {
            string termRecToWrite = "";
            string nameDict = "Dictionary";
            if (m_stemOn)
                nameDict = "Stemed-Dictionary";

            using (FileStream DicFile = new FileStream(path + "\\"  + nameDict + ".txt", FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(DicFile))
                {
                    if(m_Dictionary == null || m_Dictionary.Count == 0)
                    {
                        ModelChanged("Dictionary hasn't been created yet :(");
                        return;
                    }
                    foreach (string term in m_Dictionary.Keys)
                    {
                        termRecToWrite = "";
                        termRecToWrite += term + " ";
                        termRecToWrite += m_Dictionary[term]; // position
                        termRecToWrite += '\n';
                        bw.Write(termRecToWrite);
                    }
                }
            }
            ModelChanged("Dictionary was saved successfully on " + path + " by the name Dictionary.txt");
        }
        /// <summary>
        /// get languages
        /// </summary>
        /// <returns></returns>
        public HashSet<string> getLangs()
        {
            return m_Langs;
        }
        /// <summary>
        /// get languages
        /// </summary>
        /// <returns></returns>
        private HashSet<string> getLangss()
        {
           HashSet<string> langs=  m_readFile.getLangs();
            if(langs == null || langs.Count == 0)
            {
                return null;
            }
            HashSet<string> langsN = new HashSet<string>();
            string cleanLang;
            foreach(string lang in langs)
            {
                cleanLang = lang.Replace(" ", string.Empty);
                if (cleanLang.Length <= 17 /*&& cleanLang != "UNKNOWN"*/)
                {
                    langsN.Add(cleanLang);
                }
            }
            return langsN;
        }
        /// <summary>
        /// resets the program
        /// </summary>
        /// <param name="pathToIndOnDisk"></param>
        public void reset(string pathToIndOnDisk)
        {
            if (pathToIndOnDisk != "")
            {

                string[] files = Directory.GetFiles(pathToIndOnDisk);
                foreach (string file in files) //deletes posting files from disk
                {
                    if (file.EndsWith("Stemed-PostingF.txt") || file.EndsWith("PostingF.txt"))
                    {
                        File.Delete(file);
                    }
                }
                m_readFile.reset(); //resets data structures in the memory
            }
            m_Dictionary = null;
            m_DictionaryTF = null;
            m_docLangDict = null;
            m_docsLength = null;
            m_fileSearchResults = null;
            m_Langs = null;
            m_pathToPosting = "";
        }
        /// <summary>
        /// load dictionary
        /// </summary>
        /// <param name="pathToDict"></param>
        /// <param name="pathToDocsLength"></param>
        /// <param name="nPath"></param>
        /// <param name="postingPath"></param>
        /// <param name="docLangDict"></param>
        /// <param name="langs"></param>
        /// <param name="dicTFpath"></param>
        public void LoadDictionary(string pathToDict, string pathToDocsLength, string nPath, string postingPath, string docLangDict,string langs,string dicTFpath)
        {
            if(pathToDict == "Stemed-Dictionary.txt") //if loaded with stem on
            {
                m_stemOn = true;
            }
            m_Dictionary = new Dictionary<string, long>();
            using (FileStream DicFile = new FileStream(pathToDict, FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(DicFile))
                {
                    string line; long position = -1; string[] splitedLine; string term;
                    while (br.BaseStream.Position != br.BaseStream.Length)
                    {
                        line = br.ReadString();
                        splitedLine = line.Split(' ');
                        term = splitedLine[0];
                        long.TryParse(splitedLine[1], out position);
                        try
                        {
                            m_Dictionary.Add(term, position);
                        }
                        catch (Exception) { }
                    }
                }
            }
            Load_N(nPath);
            Load_DocsLength(pathToDocsLength);
            Load_DocsLangDict(docLangDict);
            LoadLangs(langs);
            LoadDicTF(dicTFpath);
            m_pathToPosting = postingPath;
            ModelChanged("Files were loaded successfuly :)");
        }
        /// <summary>
        /// Load Languages
        /// </summary>
        /// <param name="langs"></param>
        private void LoadLangs(string langs)
        {
            m_Langs = new HashSet<string>();
            using (FileStream langsFile = new FileStream(langs, FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(langsFile))
                {
                    while (br.BaseStream.Position != br.BaseStream.Length)
                    {
                        string rec = br.ReadString();
                        m_Langs.Add(rec);
                    }
                }
            }
        }
    }
}
