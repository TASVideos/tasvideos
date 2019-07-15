using System.IO;
using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers
{
    [FileExtension("ltm")]
    internal class Ltm : ParserBase, IParser
    {
        public override string FileExtension => "ltm";
        
        public IParseResult Parse(Stream file)
        {
            var result = new ParseResult
            {
                Region = RegionType.Ntsc,
                FileExtension = FileExtension
            };
            
            return result;
        }
    }
}