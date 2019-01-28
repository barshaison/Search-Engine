using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE_WPF.Model
{
    /// <summary>
    /// This class represents a file reader, it reads and extract documents from files
    /// </summary>
    class ReadFile
    {
        private Dictionary<string, long> m_Dic;// dic with [term : ptr]
        private Dictionary<string, long> m_DicTotalTF;
        private Indexer m_indexer;
        private int m_numOfTermsIndexed;
        private int m_numOfDocsInCorpus;
        private bool m_indexCreated; //flag to know if pressed create index
        private Dictionary<string/*docID*/, int/*doc length*/> m_docsLength;
        private Dictionary<string, string> m_docLangDict;
        /// <summary>
        /// c'tor
        /// </summary>
        public ReadFile()
        {
            m_indexCreated = false;
            m_numOfDocsInCorpus = 0;
            m_docsLength = new Dictionary<string, int>();
            m_docLangDict = new Dictionary<string, string>();
        }
        /// <summary>
        /// gets documents length
        /// </summary>
        /// <returns></returns>
       public Dictionary<string/*docID*/, int/*doc length*/> getDocsLength()
        {
            return m_docsLength;
        }
        /// <summary>
        /// get Doc Lang Dict
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,string> getDocLangDict()
        {
            return m_docLangDict;
        }
        /// <summary>
        /// reads files, extracts documents from them and sends them to the parser and from ther ti indexer
        /// </summary>
        /// <param name="pathToCorpus"></param>
        /// <param name="pathToStopWords"></param>
        /// <param name="stemOn"></param>
        /// <param name="savePosOn"></param>
        public void Readfiles(string pathToCorpus, string pathToStopWords, bool stemOn,string savePosOn)
        {
            m_numOfDocsInCorpus = 0;
            Parse parser = new Parse(pathToStopWords, stemOn);
             m_indexer = new Indexer();
            //int i = 0;
            string[] files = Directory.GetFiles(pathToCorpus);
            for (int j = 0; j <files.Length; j++)
            {
                string fileContent = File.ReadAllText(files[j]); //reads entire file into string
                List<string/*doc content*/> docs = seperateDocs(fileContent);
                parser.InitParser();
                List<List<string>> parsedTermsOfFile = parser.MakeTerms(docs); //parse single file
                addDocsLength(parsedTermsOfFile); //add docs length to data struct
                m_indexer.initIndexer(); //resets the posting structure
                m_indexer.createIndex(parsedTermsOfFile,savePosOn); //create posting for one file and adds it to queue
                //i++;
                //Console.WriteLine(i);
            }
            m_indexer.initIndexer();
            m_indexer.createFinalPostingFile(stemOn, savePosOn);
            m_indexCreated = true;
            m_Dic = m_indexer.getDictionary(); //gets the dic [term : ptr]
            m_DicTotalTF = m_indexer.getDicTF(); //gets the TF dic
            m_numOfTermsIndexed = m_DicTotalTF.Count(); //gets num of terms indexed
            m_docLangDict = m_indexer.getDocLangDict();


        }
        //add Docs Length
        private void addDocsLength(List<List<string>> parsedTermsOfFile)
        {
            
            foreach (List<string> list in parsedTermsOfFile)
            {
                m_docsLength.Add(list[0], (list.Count - 4));
            }
        }
        //get Langs
        public HashSet<string> getLangs()
        {
            if (m_indexCreated)
                return m_indexer.getDocsLanguages();
            else
                return null;
        }
        /// <summary>
        /// get Num Of Terms Indexed
        /// </summary>
        /// <returns></returns>
        public int getNumOfTermsIndexed()
        {
            return m_numOfTermsIndexed;
        }
        /// <summary>
        /// resets indexer's data structs
        /// </summary>
        public void reset()
        {
            m_indexer.resetDataStructs();
        }
        /// <summary>
        /// returns the dic [term : ptr]
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, long>  geDictionary() 
        {
            return m_Dic;
        }
        /// <summary>
        /// returns the dic [term : ptr]
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, long> geDictionaryTF() 
        {
            return m_DicTotalTF;
        }
        /// <summary>
        /// get Num Of Docs In Corpus
        /// </summary>
        /// <returns></returns>
        public int getNumOfDocsInCorpus()
        {
            return m_numOfDocsInCorpus;
        }
        //returns list of documents (strings with doc content)
        private List<string> seperateDocs(string fileContent)
        {
            List<string> docsInfile = new List<string>();
            bool stop = false;
            int startIndex = 0;
            int endIndex = 0;
            while (!stop)
            {
                string currDoc;
                startIndex = fileContent.IndexOf("<DOC>", startIndex) + 5;
                endIndex = fileContent.IndexOf("</DOC>", endIndex);
                if (startIndex == -1 || endIndex == -1)
                {
                    break;
                }

                currDoc = fileContent.Substring(startIndex, endIndex - startIndex); //takes the text between <DOC> and </DOC>
                docsInfile.Add(currDoc);
                m_numOfDocsInCorpus++; //counts num of docs in corpus
                startIndex++;
                endIndex++;
                if ((endIndex + 5) >= fileContent.Length - 1 || fileContent.Length == 0)
                {
                    stop = true;
                }

            }
            return docsInfile;
        }
    }
}
