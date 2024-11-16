using Bogus;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Spell;
using Lucene.Net.Search.Suggest.Analyzing;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System.Text.RegularExpressions;

namespace search.Shared
{
    public class SearchEngine{
        public static List<WaffleText> Data {get; set;} = new List<WaffleText>();
        private static RAMDirectory _directory = new RAMDirectory();
        public static IndexWriter Writer { get; set; }

        public static void GetData(int Rand, int WaffleCount)
        {
            Randomizer.Seed = new Random(Rand);
            var testWaffles = new Faker<WaffleText>()
                .RuleFor(wt => wt.GUID, f => Guid.NewGuid().ToString())
                .RuleFor(
                    property: wt => wt.WaffleHead,
                    setter: (f, wt) => f.WaffleTitle())
                .RuleFor(
                    property: wt => wt.WaffleBody,
                    setter: (f, wt) => f.WaffleText(
                        paragraphs: 2,
                        includeHeading: false));
            var waffles = testWaffles.Generate(WaffleCount);
            
            Data = new List<WaffleText>();
            foreach(WaffleText wt in waffles)
            {
                Data.Add(wt);
            }
        }

        public static void Index()
        {
            const LuceneVersion lv = LuceneVersion.LUCENE_48;
            Analyzer a = new StandardAnalyzer(lv);
            _directory = new RAMDirectory();
            var config = new IndexWriterConfig(lv, a);
            Writer = new IndexWriter(_directory, config);

            var guidField = new StringField("GUID", "", Field.Store.YES);
            var headField = new TextField("WaffleHead", "", Field.Store.YES);
            var bodyField = new TextField("WaffleBody", "", Field.Store.YES);
            // Hoping for better way to do this than indexing combined field
            var headBody = new TextField("HeadBody", "", Field.Store.YES);

            var d = new Document()
            {
                guidField,
                headField,
                bodyField,
                headBody
            };

            foreach (WaffleText wt in Data)
            {
                guidField.SetStringValue(wt.GUID);
                headField.SetStringValue(wt.WaffleHead);
                bodyField.SetStringValue(wt.WaffleBody);
                headBody.SetStringValue(wt.WaffleHead + " " + wt.WaffleBody);
                Writer.AddDocument(d);
            }
            Writer.Commit();
        }

        public static void Dispose()
        {
            Writer.Dispose();
            _directory.Dispose();
        }

        public static SearchModel Search(string input, int page)
        {
            const LuceneVersion lv = LuceneVersion.LUCENE_48;
            Analyzer a = new StandardAnalyzer(lv);
            var dirReader = DirectoryReader.Open(_directory);
            var searcher = new IndexSearcher(dirReader);

            string[] waffles = { "GUID", "WaffleHead", "WaffleBody" };
            var multiFieldQP = new MultiFieldQueryParser(lv, waffles, a);
            string _input = EscapeSearchTerm(input.Trim());
            Query query = multiFieldQP.Parse(_input);

            ScoreDoc[] docs = searcher.Search(query, null, 1000).ScoreDocs;

            var returnModel = new SearchModel();
            returnModel.CurrentPageSearchResults = new List<WaffleText>();
            returnModel.SearchText = _input;
            returnModel.ResultsCount = docs.Length;
            returnModel.PageCount = (int)Math.Ceiling(docs.Length/5.0);
            returnModel.CurrentPage = page;

            int first = (page-1)*5;
            int last = first + 5;
            for (int i = first; i < last && i < docs.Length; i++)
            {
                Document d = searcher.Doc(docs[i].Doc);
                WaffleText _localWaffle = new WaffleText();
                _localWaffle.GUID = d.Get("GUID");
                _localWaffle.WaffleHead = d.Get("WaffleHead");
                _localWaffle.WaffleBody = d.Get("WaffleBody");
                returnModel.CurrentPageSearchResults.Add(_localWaffle);
            }
            dirReader.Dispose();
            return returnModel;
        }

        public static List<string> SearchAhead(string input)
        {
            const LuceneVersion lv = LuceneVersion.LUCENE_48;
            Analyzer a = new StandardAnalyzer(lv);
            var dirReader = DirectoryReader.Open(_directory);

            LuceneDictionary dictionary = new LuceneDictionary(dirReader, "HeadBody");

            RAMDirectory _d = new RAMDirectory();
            AnalyzingInfixSuggester analyzingSuggester = new AnalyzingInfixSuggester(lv, _d, a);
            analyzingSuggester.Build(dictionary);

            var lookupResultList = analyzingSuggester.DoLookup(input.Trim(), false, 9);

            List<string> returnModel = new List<string>();
            foreach(var result in lookupResultList)
            {
                returnModel.Add(result.Key);
            }

            return returnModel;
            dirReader.Dispose();
        }

        // Lucene supports escapting the following chars: + - && || ! ( ) { } [ ] ^ " ~ * ? : \
        // To make it easier, I remove / replace the text altogether
        // Added bold html tag replacement for type ahead
        private static string EscapeSearchTerm(string input)
        {
            input = Regex.Replace(input, @"<b>", "");
            input = Regex.Replace(input, @"</b>", "");
            input = Regex.Replace(input, @"\+", " ");
            input = Regex.Replace(input, @"\-", " ");
            input = Regex.Replace(input, @"\&", " ");
            input = Regex.Replace(input, @"\|", " ");
            input = Regex.Replace(input, @"\!", " ");
            input = Regex.Replace(input, @"\(", " ");
            input = Regex.Replace(input, @"\)", " ");
            input = Regex.Replace(input, @"\{", " ");
            input = Regex.Replace(input, @"\}", " ");
            input = Regex.Replace(input, @"\[", " ");
            input = Regex.Replace(input, @"\]", " ");
            input = Regex.Replace(input, @"\^", " ");
            input = Regex.Replace(input, @"\""", " ");
            input = Regex.Replace(input, @"\~", " ");
            input = Regex.Replace(input, @"\*", " ");
            input = Regex.Replace(input, @"\?", " ");
            input = Regex.Replace(input, @"\:", " ");
            input = Regex.Replace(input, @"\\", " ");
            return input;
        }
    }
}