using System.Collections.Generic;
using System.Linq;

namespace TestContainers.Containers.Reaper.Filters
{
    /// <summary>
    /// A filter that filters by labels
    /// </summary>
    /// <inheritdoc />
    public class LabelsFilter : IFilter
    {
        private readonly IDictionary<string, string> _labels;

        /// <inheritdoc />
        public LabelsFilter(string name, string value)
        {
            _labels = new Dictionary<string, string> {{name, value}};
        }

        /// <inheritdoc />
        public LabelsFilter(IDictionary<string, string> labels)
        {
            _labels = labels;
        }

        /// <inheritdoc />
        public string ToFilterString()
        {
            return _labels
                .Select(note => $"label={note.Key}={note.Value}")
                .Aggregate((current, next) => current + "&" + next);
        }
    }
}
