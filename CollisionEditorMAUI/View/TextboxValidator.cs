using System.Collections.Generic;
using System.ComponentModel;
using System.Collections;
using System.Linq;
using System;

namespace CollisionEditor.View
{
    internal class TextboxValidator : INotifyDataErrorInfo
    {
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public bool HasErrors => propertyErrors.Any();

        private readonly Dictionary<string, List<string>?> propertyErrors = new();

        public IEnumerable GetErrors(string? propertyName)
        {
            if (propertyName is null)
                return new Dictionary<string, List<string>?>();

            List<string>? errors = propertyErrors.GetValueOrDefault(propertyName, null);
            return errors is null ? new List<string>() : errors;
        }

        public void AddError(string propertyName, string errorMessage)
        {
            if (!propertyErrors.ContainsKey(propertyName))
                propertyErrors.Add(propertyName, new List<string>());

            propertyErrors[propertyName]?.Add(errorMessage);
            OnErrorsChanged(propertyName);
        }

        private void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public void ClearErrors(string propertyName)
        {
            if (propertyErrors.Remove(propertyName))
                OnErrorsChanged(propertyName);
        }
    }
}
