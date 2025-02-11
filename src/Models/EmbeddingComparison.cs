using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mistral.Models
{
    internal class EmbeddingComparison
    {
        public long Id { get; set; }

        public string Content { get; set; } = String.Empty;

        public float Similarity { get; set; }
    }
}
