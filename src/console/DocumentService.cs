using LangChain.Extensions;
using Mistral.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mistral
{
    internal class DocumentService
    {
        private readonly AppSettings _appSettings;

        public DocumentService(AppSettings appSettings)
        {
            this._appSettings = appSettings;
        }
        
        public List<string> SplitDocument()
        {
            string[] text = { File.ReadAllText(_appSettings.CricketLawChangeProposalDocumentPath) };
            var texts = new ReadOnlyCollection<string>(text);

            var splitter = new LangChain.Splitters.Text.RecursiveCharacterTextSplitter(chunkSize: 270, chunkOverlap: 70);

            var splitDocuments = splitter.CreateDocuments(texts);
            return splitDocuments.Select(x => x.PageContent).ToList();
        }


    }
}
