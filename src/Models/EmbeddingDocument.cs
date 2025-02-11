using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mistral.Models
{
    internal class EmbeddingDocument
    {
        public int Index { get; set; }

        public string Content { get; set; } = String.Empty;

        public List<decimal> Embedding { get; set; } = new List<decimal>();
    }
}
