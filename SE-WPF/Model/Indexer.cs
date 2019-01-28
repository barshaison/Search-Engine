using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE_WPF.Model
{
    /// <summary>
    /// This class represents an indexer that creats index for given corpus
    /// </summary>
    class Indexer
    {
        SortedDictionary<string/*term*/, Dictionary<string/*docId*/,/*positions*/ List<int>>> m_posting; // <term> , <docID>, positions of term in the doc: [ 1, 3 ,4 ]
        static int m_fileNumber = 0; // name of curr posting file
        static Dictionary<string/*term*/, long/*ptr to pos rec*/> m_Dictionary = new Dictionary<string, long>();
        static Queue<string> m_postingQueue = new Queue<string/*file name*/>();
        static int m_mergedFileSerialNum = 0;
        static Dictionary<string/*term*/, long /*total_TF*/> m_dicToShowForPartA = new Dictionary<string, long>(); //dictionary with term:total_TF
        static HashSet<string> m_docsLanguages = new HashSet<string>(); //language dic
        private Dictionary<string/*doc*/, string /*lnaguage*/> m_docLangDict; 

        //c'tor
        public Indexer()
        {
            m_Dictionary = new Dictionary<string, long>();
            m_dicToShowForPartA = new Dictionary<string, long>();
            m_fileNumber = 0;
            m_postingQueue = new Queue<string>();
            m_mergedFileSerialNum = 0;
            m_docsLanguages = new HashSet<string>();
            m_docLangDict = new Dictionary<string, string>();

        }
        /// <summary>
        /// gets the TF dictionary
        /// </summary>
        /// <returns></returns>
        public Dictionary<string/*term*/, long /*total_TF*/> getDicTF()
        {
            return m_dicToShowForPartA;
        }

        /// <summary>
        /// returns docs Languages
        /// </summary>
        /// <returns></returns>
        public HashSet<string> getDocsLanguages() 
        {
            return m_docsLanguages;
        }


        /// <summary>
        /// resets the posting
        /// </summary>
        public void initIndexer()
        {
            m_posting = new SortedDictionary<string, Dictionary<string, List<int>>>();
            
        }

        /// <summary>
        /// resets data structs
        /// </summary>
        public void resetDataStructs()
        {
            m_posting = new SortedDictionary<string, Dictionary<string, List<int>>>();
            m_fileNumber = 0;
            m_Dictionary = new Dictionary<string, long>();
            m_postingQueue =new Queue<string>();
            m_mergedFileSerialNum = 0;
            m_docsLanguages = new HashSet<string>();
            m_dicToShowForPartA = new Dictionary<string, long>();

        }
        /// <summary>
        /// save the dictionary on disk
        /// </summary>
        /// <param name="saveDicOn">path</param>
        public void saveDicOnDisc(string saveDicOn)
        {
            string termRecToWrite = "";
            
            using (FileStream DicFile = new FileStream(saveDicOn + /*  / */"Dictionary" + ".txt", FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(DicFile))
                {
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
        }

        /// <summary>
        /// load dictionary from disk
        /// </summary>
        /// <param name="path">path</param>
        public void LoadDicFromDisk(string path)
        {
            m_Dictionary = new Dictionary<string, long>();

            using (FileStream DicFile = new FileStream(path, FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(DicFile))
                {
                    string line; long position=-1; string[] splitedLine; string term;
                    while (br.BaseStream.Position != br.BaseStream.Length)
                    {
                        line = br.ReadString();
                        splitedLine = line.Split(' ');
                        term = splitedLine[0];
                        long.TryParse(splitedLine[1], out position);
                        m_Dictionary.Add(term, position);
                    }
                }
            }
            
        }

        /// <summary>
        /// get the docs langs dictionary
        /// </summary>
        /// <returns>dictionary</returns>
        public Dictionary<string, string> getDocLangDict()
        {
            return m_docLangDict;
        }
        /// <summary>
        /// gets the dictionary
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, long> getDictionary()
        {
            return m_Dictionary;
        }
        /// <summary>
        /// creates index for one file
        /// </summary>
        /// <param name="file">files parsed words</param>
        /// <param name="savePosOn">path</param>
        public void createIndex(List<List<String>> file, string savePosOn) 
        {
            string docID; int position_Of_Term_In_Curr_Doc;

            foreach (List<string> currDoc in file)
            {
                position_Of_Term_In_Curr_Doc = 0; //reset position of term in curr doc
                docID = currDoc[0]; //extract the doc ID from the first place in the list

                m_docsLanguages.Add(currDoc[1]); //add language to langDictionary [!!!!!]
                m_docLangDict.Add(docID, currDoc[1]); //add docID and its language
                string currTerm;
                for (int i = 4; i < currDoc.Count; i++) //first term is in the 4th place in the list
                {
                    currTerm = currDoc[i];

                    if (!m_posting.Keys.Contains(currDoc[i])) // curr term hasn't been added yet to the posting
                    {
                        Dictionary<string, List<int>> termRec = new Dictionary<string, List<int>>(); //create new term record
                        List<int> positions = new List<int>();
                        positions.Add(position_Of_Term_In_Curr_Doc);
                        termRec.Add(docID, positions);
                        m_posting.Add(currTerm, termRec);
                        position_Of_Term_In_Curr_Doc++;
                    }

                    else //curr term already exists in the posting
                    {
                        if (!m_posting[currTerm].Keys.Contains(docID)) //curr term wasn't seen in the doc yet
                        {
                            List<int> positions = new List<int>();
                            positions.Add(position_Of_Term_In_Curr_Doc);
                            m_posting[currTerm].Add(docID, positions);
                            position_Of_Term_In_Curr_Doc++;
                        }
                        else // curr term was already seen in the doc
                        {
                            m_posting[currTerm][docID].Add(position_Of_Term_In_Curr_Doc);
                            position_Of_Term_In_Curr_Doc++;
                        }

                    }
                }
            }

            writePostingToFile(savePosOn);
        }

        /// <summary>
        /// merges all the posting files in the queue and creates the dictionary
        /// </summary>
        /// <param name="stem">stem option</param>
        /// <param name="savePosOn">path</param>
        public void createFinalPostingFile(bool stem, string savePosOn)
        {
            while (m_postingQueue.Count > 2)
            {
                string file1 = m_postingQueue.Dequeue();
                string file2 = m_postingQueue.Dequeue();
                merge_Two_Posting_Files(file1, file2, savePosOn);
            }

            createDictionary(stem, savePosOn);
        }
        /// <summary>
        /// counts total tf for the dic to show
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private int countTotalTF(string line) 
        {
            int tf = 0;
            string[] splited = line.Split(' '); long num;
            foreach (string a in splited)
            {
                if (long.TryParse(a, out num))
                {
                    tf++;
                }
            }

            return tf;
        }

        /// <summary>
        /// merge the remaining 2 files in the queue and creates the dictionary
        /// </summary>
        /// <param name="stem">stem option</param>
        /// <param name="savePosOn">path</param>
        private void createDictionary(bool stem,string savePosOn)
        {
            string file1 = m_postingQueue.Dequeue();
            string file2 = m_postingQueue.Dequeue();
            using (Stream s1 = new FileStream(file1, FileMode.Open))
            {
                using (Stream s2 = new FileStream(file2, FileMode.Open))
                {
                    string PostingFile;
                    if (stem) //not to over ride the file
                    {
                        PostingFile = "Stemed-PostingF.txt";
                    }
                    else
                    {
                        PostingFile = "PostingF.txt";
                    }
                    
                    using (Stream mergedFile = new FileStream(savePosOn + "\\" + PostingFile, FileMode.Create))
                    {
                        using (BinaryReader br1 = new BinaryReader(s1))
                        {
                            using (BinaryReader br2 = new BinaryReader(s2))
                            {
                                using (BinaryWriter merged = new BinaryWriter(mergedFile))
                                {
                                    bool start = true;
                                    string line1 = "", line2 = "", term1 = null, term2 = null;
                                    int comp;
                                    long position = merged.BaseStream.Position;
                                    while (br1.BaseStream.Position != br1.BaseStream.Length && br2.BaseStream.Position != br2.BaseStream.Length)
                                    {

                                        if (start)
                                        {
                                            line1 = br1.ReadString();
                                            line2 = br2.ReadString();
                                            term1 = line1.Substring(0, line1.IndexOf('@'));
                                            term2 = line2.Substring(0, line2.IndexOf('@'));
                                            start = false;
                                            continue;
                                        }

                                        comp = string.Compare(term1, term2);
                                        if (comp < 0) //t1 < t2
                                        {
                                            try { m_Dictionary.Add(term1, position); }
                                            catch (Exception) { }
                                            try { m_dicToShowForPartA.Add(term1, countTotalTF(line1)); }
                                            catch (Exception) { }

                                            merged.Write(line1);
                                            position = merged.BaseStream.Position;
                                            line1 = br1.ReadString();
                                            term1 = line1.Substring(0, line1.IndexOf('@'));
                                        }
                                        else if (comp > 0) //t1 > t2
                                        {
                                            try { m_Dictionary.Add(term2, position); }
                                            catch (Exception) {  }
                                            try { m_dicToShowForPartA.Add(term2, countTotalTF(line2)); }
                                            catch (Exception) { }
                                            merged.Write(line2);
                                            position = merged.BaseStream.Position;
                                            line2 = br2.ReadString();
                                            term2 = line2.Substring(0, line2.IndexOf('@'));
                                        }
                                        else //t1 = t2
                                        {
                                            line1 += "|" + line2.Substring(line2.IndexOf('@') + 1);
                                            try { m_Dictionary.Add(term1, position); }
                                            catch (Exception) {  }
                                            try { m_dicToShowForPartA.Add(term1, countTotalTF(line1)); }
                                            catch (Exception) { }
                                            merged.Write(line1);
                                            position = merged.BaseStream.Position;
                                            line1 = br1.ReadString();
                                            line2 = br2.ReadString();
                                            term1 = line1.Substring(0, line1.IndexOf('@'));
                                            term2 = line2.Substring(0, line2.IndexOf('@'));
                                        }
                                    }
                                    string tt1 = null, tt2 = null;
                                    comp = string.Compare(term1, term2);
                                    if (comp < 0) //t1 < t2
                                    {
                                        try { m_Dictionary.Add(term1, position); }
                                        catch (Exception) { }
                                        try { m_dicToShowForPartA.Add(term1, countTotalTF(line1)); }
                                        catch (Exception) { }
                                        merged.Write(line1);
                                        position = merged.BaseStream.Position;
                                        tt2 = term2;
                                    }
                                    else if (comp > 0) //t1 > t2
                                    {
                                        try { m_Dictionary.Add(term2, position); }
                                        catch (Exception) {  }
                                        try { m_dicToShowForPartA.Add(term2, countTotalTF(line2)); }
                                        catch (Exception) { }
                                        merged.Write(line2);
                                        position = merged.BaseStream.Position;
                                        tt1 = term1;
                                    }
                                    else
                                    {
                                        line1 += "|" + line2.Substring(line2.IndexOf('@') + 1);
                                        try { m_Dictionary.Add(term1, position); }
                                        catch (Exception) { }
                                        try { m_dicToShowForPartA.Add(term1, countTotalTF(line1)); }
                                        catch (Exception) { }

                                        merged.Write(line1);
                                        position = merged.BaseStream.Position;
                                    }

                                    if (br1.BaseStream.Position != br1.BaseStream.Length) //try1
                                    {
                                        if (tt1 != null)
                                        {
                                            try { m_Dictionary.Add(tt1, position); }
                                            catch (Exception) { }
                                           
                                            try { m_dicToShowForPartA.Add(tt1, countTotalTF(line1)); }
                                            catch (Exception) { }
                                            merged.Write(line1);
                                            position = merged.BaseStream.Position;
                                        }
                                        line1 = br1.ReadString();
                                        tt1 = line1.Substring(0, line1.IndexOf('@'));
                                    }
                                    if (br2.BaseStream.Position != br2.BaseStream.Length) //try2
                                    {
                                        if (tt2 != null)
                                        {
                                            try { m_Dictionary.Add(tt2, position); }
                                            catch (Exception) { }
                                            
                                            try { m_dicToShowForPartA.Add(tt2, countTotalTF(line2)); }
                                            catch (Exception) { }
                                            merged.Write(line2);
                                            position = merged.BaseStream.Position;
                                        }
                                        line2 = br2.ReadString();
                                        tt2 = line2.Substring(0, line2.IndexOf('@'));
                                    }

                                    if (tt1 != null && tt2 != null)
                                    {
                                        comp = string.Compare(tt1, tt2);
                                        if (comp < 0)
                                        {
                                            try { m_Dictionary.Add(tt1, position); }
                                            catch (Exception) {  }
                                            try { m_dicToShowForPartA.Add(tt1, countTotalTF(line1)); }
                                            catch (Exception) { }
                                            merged.Write(line1);
                                            position = merged.BaseStream.Position;
                                            try { m_Dictionary.Add(tt2, position); }
                                            catch (Exception) {  }
                                            try { m_dicToShowForPartA.Add(tt2, countTotalTF(line2)); }
                                            catch (Exception) { }
                                            merged.Write(line2);
                                            position = merged.BaseStream.Position;
                                        }
                                        else if (comp > 0)
                                        {
                                            try { m_Dictionary.Add(tt2, position); }
                                            catch (Exception) {  }
                                            try { m_dicToShowForPartA.Add(tt2, countTotalTF(line2)); }
                                            catch (Exception) { }

                                            merged.Write(line2);
                                            position = merged.BaseStream.Position;
                                            try { m_Dictionary.Add(tt1, position); }
                                            catch (Exception) {  }
                                            try { m_dicToShowForPartA.Add(tt1, countTotalTF(line1)); }
                                            catch (Exception) { }
                                            merged.Write(line1);
                                            position = merged.BaseStream.Position;
                                        }
                                        else
                                        {
                                            line1 += "|" + line2.Substring(line2.IndexOf('@') + 1);
                                            try { m_Dictionary.Add(tt1, position); }
                                            catch (Exception) {  }
                                            try { m_dicToShowForPartA.Add(tt1, countTotalTF(line1)); }
                                            catch (Exception) { }

                                            merged.Write(line1);
                                            position = merged.BaseStream.Position;
                                        }
                                    }
                                    else //(tt1 || tt2 = "")
                                    {
                                        if (tt1 != null)
                                        {
                                            try { m_Dictionary.Add(tt1, position); }
                                            catch (Exception) {  }
                                            try { m_dicToShowForPartA.Add(tt1, countTotalTF(line1)); }
                                            catch (Exception) { }
                                            merged.Write(line1);
                                            position = merged.BaseStream.Position;
                                        }
                                        if (tt2 != null)
                                        {
                                            try { m_Dictionary.Add(tt2, position); }
                                            catch (Exception) {  }
                                            try { m_dicToShowForPartA.Add(tt2, countTotalTF(line2)); }
                                            catch (Exception) { }
                                            merged.Write(line2);
                                            position = merged.BaseStream.Position;
                                        }
                                    }

                                    // write the remains of file1
                                    while (br1.BaseStream.Position != br1.BaseStream.Length)
                                    {
                                        line1 = br1.ReadString();
                                        tt1 = line1.Substring(0, line1.IndexOf('@'));
                                        try { m_Dictionary.Add(tt1, position); }
                                        catch (Exception) { }
                                        try { m_dicToShowForPartA.Add(tt1, countTotalTF(line1)); }
                                        catch (Exception) { }

                                        merged.Write(line1);
                                        position = merged.BaseStream.Position;
                                    }
                                    // write the remains of file2
                                    while (br2.BaseStream.Position != br2.BaseStream.Length)
                                    {
                                        line2 = br2.ReadString();
                                        tt2 = line2.Substring(0, line2.IndexOf('@'));
                                        try
                                        {
                                            m_Dictionary.Add(tt2, position);
                                        }
                                        catch (Exception) { }
                                        try { m_dicToShowForPartA.Add(tt2, countTotalTF(line2)); }
                                        catch (Exception) { }

                                        merged.Write(line2);
                                        position = merged.BaseStream.Position;
                                    }

                                }
                            }
                        }
                    }
                }
            }
            File.Delete(file1);
            File.Delete(file2);
        }

        /// <summary>
        /// merge 2 posting files
        /// </summary>
        /// <param name="pathToFile1">file 1 path</param>
        /// <param name="pathToFile2">file 2 path</param>
        /// <param name="savePosOn">path merged</param>
        private void merge_Two_Posting_Files(string pathToFile1, string pathToFile2, string savePosOn)
        {

            using (Stream file1 = new FileStream(pathToFile1, FileMode.Open))
            {
                using (Stream file2 = new FileStream(pathToFile2, FileMode.Open))
                {
                    int mNum = m_mergedFileSerialNum; //holds the name of the curr merged file
                    m_mergedFileSerialNum++;

                    using (Stream mergedFile = new FileStream(savePosOn + "\\" +"Merged" + mNum + ".txt", FileMode.Create))
                    {
                        m_postingQueue.Enqueue(savePosOn + "\\" + "Merged" + mNum + ".txt"); //add the merged file to the queue
                        using (BinaryReader br1 = new BinaryReader(file1))
                        {
                            using (BinaryReader br2 = new BinaryReader(file2))
                            {
                                using (BinaryWriter merged = new BinaryWriter(mergedFile))
                                {

                                    bool start = true;
                                    string line1 = "", line2 = "", term1 = null, term2 = null;
                                    int comp;
                                    while (br1.BaseStream.Position != br1.BaseStream.Length && br2.BaseStream.Position != br2.BaseStream.Length)
                                    {

                                        if (start)
                                        {
                                            line1 = br1.ReadString();
                                            line2 = br2.ReadString();
                                            term1 = line1.Substring(0, line1.IndexOf('@'));
                                            term2 = line2.Substring(0, line2.IndexOf('@'));
                                            start = false;
                                            continue;
                                        }
                                        comp = string.Compare(term1, term2);
                                        if (comp < 0) //t1 < t2
                                        {
                                            merged.Write(line1);
                                            line1 = br1.ReadString();
                                            term1 = line1.Substring(0, line1.IndexOf('@'));
                                        }
                                        else if (comp > 0) //t1 > t2
                                        {
                                            merged.Write(line2);
                                            line2 = br2.ReadString();
                                            term2 = line2.Substring(0, line2.IndexOf('@'));
                                        }
                                        else //t1 = t2
                                        {
                                            line1 += "|" + line2.Substring(line2.IndexOf('@') + 1);
                                            merged.Write(line1);
                                            line1 = br1.ReadString();
                                            line2 = br2.ReadString();
                                            term1 = line1.Substring(0, line1.IndexOf('@'));
                                            term2 = line2.Substring(0, line2.IndexOf('@'));
                                        }
                                    }
                                    string tt1 = null, tt2 = null;
                                    comp = string.Compare(term1, term2);
                                    if (comp < 0)
                                    {
                                        merged.Write(line1);
                                        tt2 = term2;
                                    }
                                    else if (comp > 0)
                                    {
                                        merged.Write(line2);
                                        tt1 = term1;
                                    }
                                    else
                                    {
                                        line1 += "|" + line2.Substring(line2.IndexOf('@') + 1);
                                        merged.Write(line1);
                                    }

                                    if (br1.BaseStream.Position != br1.BaseStream.Length) //try1
                                    {
                                        if (tt1 != null)
                                        {

                                            merged.Write(line1);

                                        }
                                        line1 = br1.ReadString();
                                        tt1 = line1.Substring(0, line1.IndexOf('@'));
                                    }
                                    if (br2.BaseStream.Position != br2.BaseStream.Length) //try2
                                    {
                                        if (tt2 != null)
                                        {

                                            merged.Write(line2);

                                        }
                                        line2 = br2.ReadString();
                                        tt2 = line2.Substring(0, line2.IndexOf('@'));
                                    }

                                    if (tt1 != null && tt2 != null)
                                    {
                                        comp = string.Compare(tt1, tt2);
                                        if (comp < 0)
                                        {
                                            merged.Write(line1);
                                            merged.Write(line2);
                                        }
                                        else if (comp > 0)
                                        {
                                            merged.Write(line2);
                                            merged.Write(line1);
                                        }
                                        else
                                        {
                                            line1 += "|" + line2.Substring(line2.IndexOf('@') + 1);
                                            merged.Write(line1);
                                        }
                                    }
                                    else //(tt1 || tt2 = "")
                                    {
                                        if (tt1 != null) merged.Write(line1);
                                        if (tt2 != null) merged.Write(line2);
                                    }

                                    // write the remains of file1
                                    while (br1.BaseStream.Position != br1.BaseStream.Length)
                                    {
                                        line1 = br1.ReadString();
                                        //tt1 = line1.Substring(0, line1.IndexOf('@'));
                                        merged.Write(line1);
                                    }
                                    // write the remains of file2
                                    while (br2.BaseStream.Position != br2.BaseStream.Length)
                                    {
                                        line2 = br2.ReadString();
                                        //tt2 = line2.Substring(0, line2.IndexOf('@'));
                                        merged.Write(line2);
                                    }
                                }

                            }
                        }
                    }
                }
            }
            File.Delete(pathToFile1);
            File.Delete(pathToFile2);
        }

        /// <summary>
        /// writes current posting to a file in form of strings
        /// </summary>
        /// <param name="path">path</param>
        private void writePostingToFile(string path)
        {
            string termRecToWrite = "";
            int fileSerialNum = m_fileNumber;
            m_fileNumber++;
            using (FileStream posFile = new FileStream(path + "\\" +"posting" + fileSerialNum + ".txt", FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(posFile))
                {
                    foreach (string term in m_posting.Keys)
                    {
                        termRecToWrite = "";
                        termRecToWrite += term + "@ ";
                        foreach (string docID in m_posting[term].Keys)
                        {
                            termRecToWrite += docID + "# ";
                            foreach (int place in m_posting[term][docID])
                            {
                                termRecToWrite += place + " ";
                            }
                            termRecToWrite += "| ";
                        }
                        termRecToWrite += '\n';
                        bw.Write(termRecToWrite);
                    }
                }
            }
            m_postingQueue.Enqueue(path + "\\" + "posting" + fileSerialNum + ".txt");
        }
    }
}
