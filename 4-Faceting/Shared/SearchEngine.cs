using Bogus;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Documents;
using Lucene.Net.Facet;
using Lucene.Net.Facet.Taxonomy;
using Lucene.Net.Facet.Taxonomy.Directory;
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
        private static RAMDirectory _indexDirectory = new RAMDirectory();
        private static RAMDirectory _facetDirectory = new RAMDirectory();
        public static IndexWriter? indexWriter { get; set; }
        public static DirectoryTaxonomyWriter? taxoWriter { get; set; }
        private static FacetsConfig facetConfig = new FacetsConfig();


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
                        includeHeading: false))
                .RuleFor(wt => wt.WaffleScholar, f => f.PickRandom<WaffleScholar>())
                .RuleFor(wt => wt.WaffleUniversity, f => f.PickRandom<WaffleUniversity>());
            
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
            _indexDirectory = new RAMDirectory();
            _facetDirectory = new RAMDirectory();
            var config = new IndexWriterConfig(lv, a);

            indexWriter = new IndexWriter(_indexDirectory, config);
            taxoWriter = new DirectoryTaxonomyWriter(_facetDirectory);

            var doc = new Document();

            foreach (WaffleText wt in Data)
            {
                doc = new Document();
                doc.Add(new StringField("GUID", wt.GUID, Field.Store.YES));
                doc.Add(new TextField("WaffleHead", wt.WaffleHead, Field.Store.YES));
                doc.Add(new TextField("WaffleBody", wt.WaffleBody, Field.Store.YES));
                doc.Add(new TextField("HeadBody", wt.WaffleHead + " " + wt.WaffleBody, Field.Store.YES));
                doc.Add(new TextField("WaffleScholarTxt", wt.WaffleScholar.ToString(), Field.Store.YES));
                doc.Add(new TextField("WaffleUniversityTxt", wt.WaffleUniversity.ToString(), Field.Store.YES));
                doc.Add(new FacetField("WaffleScholar", wt.WaffleScholar.ToString()));
                doc.Add(new FacetField("WaffleUniversity", wt.WaffleUniversity.ToString()));

                indexWriter.AddDocument(facetConfig.Build(taxoWriter, doc));
            }
            indexWriter.Commit();
            taxoWriter.Commit();
        }

        public static void Dispose()
        {
            indexWriter?.Dispose();
            taxoWriter?.Dispose();
            _indexDirectory?.Dispose();
            _facetDirectory?.Dispose();
        }

        public static SearchModel FacetedSearch(string input, int page, List<string> scholarDrillDowns = null, List<string> universityDrillDowns = null)
        {
            const LuceneVersion lv = LuceneVersion.LUCENE_48;
            Analyzer a = new StandardAnalyzer(lv);
            
            string[] fnames = { "GUID", "WaffleHead", "WaffleBody" };
            var multiFieldQP = new MultiFieldQueryParser(lv, fnames, a);
            string _input = EscapeSearchTerm(input.Trim());
            Query query = multiFieldQP.Parse(_input);

            // Add drill down query
            DrillDownQuery ddq = new DrillDownQuery(facetConfig, query);
            if (scholarDrillDowns is not null)
            {
                foreach (string scholar in scholarDrillDowns)
                {
                    ddq.Add("WaffleScholar", scholar);
                }
            }
            if (universityDrillDowns is not null)
            {
                foreach (string university in universityDrillDowns)
                {
                    ddq.Add("WaffleUniversity", university);
                }
            }
            
            using (DirectoryReader indexReader = DirectoryReader.Open(_indexDirectory))
            using (TaxonomyReader taxoReader = new DirectoryTaxonomyReader(_facetDirectory))
            {
                IndexSearcher searcher = new IndexSearcher(indexReader);

                // Execute document search and return collection of WaffleText class
                ScoreDoc[] docs = searcher.Search(ddq, null, 1000).ScoreDocs;
                var waffles = new List<WaffleText>();
                int first = (page-1)*5;
                int last = first + 5;
                for (int i = first; i < last && i < docs.Length; i++)
                {
                    Document doc = searcher.Doc(docs[i].Doc);
                    WaffleText _waffle = new WaffleText();
                    _waffle.GUID = doc.Get("GUID");
                    _waffle.WaffleHead = doc.Get("WaffleHead");
                    _waffle.WaffleBody = doc.Get("WaffleBody");
                    _waffle.WaffleScholar = (WaffleScholar)Enum.Parse(typeof(WaffleScholar), doc.Get("WaffleScholarTxt"));
                    _waffle.WaffleUniversity = (WaffleUniversity)Enum.Parse(typeof(WaffleUniversity), doc.Get("WaffleUniversityTxt"));
                    waffles.Add(_waffle);
                }

                var returnModel = new SearchModel();
                returnModel.CurrentPageSearchResults = waffles;
                returnModel.SearchText = _input;
                returnModel.ResultsCount = docs.Length;
                returnModel.PageCount = (int)Math.Ceiling(docs.Length/5.0);
                returnModel.CurrentPage = page;

                // Execute facets search and return collection of FacetResults class
                FacetsCollector fc = new FacetsCollector();
                FacetsCollector.Search(searcher, ddq, 100, fc);
                IList<FacetResult> results = new List<FacetResult>();
                Facets facets = new FastTaxonomyFacetCounts(taxoReader, facetConfig, fc);
                results.Add(facets.GetTopChildren(100, "WaffleScholar"));
                results.Add(facets.GetTopChildren(100, "WaffleUniversity"));

                returnModel.FacetResults = results;

                return returnModel;
            }
        }

        public static List<string> SearchAhead(string input)
        {
            const LuceneVersion lv = LuceneVersion.LUCENE_48;
            Analyzer a = new StandardAnalyzer(lv);
            var dirReader = DirectoryReader.Open(_indexDirectory);

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