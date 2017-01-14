using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FMSC.Core.ComponentModel
{
    public class PropertyWatcher<T> : INotifyPropertyChanged where T : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Dictionary<string, PropertyChangedEventHandler> _WatchedProperties;

        public PropertyChangedEventHandler this[string key] { get { return _WatchedProperties[key]; } }


        public PropertyWatcher(T obj, Expression<Func<T, object>> propertiesToWatch = null)
        {
            RegisterPropertiesWatcher(propertiesToWatch);

            obj.PropertyChanged += PropertyChangedHandler;
        }

        public PropertyWatcher(IEnumerable<T> objs, Expression<Func<T, object>> propertiesToWatch = null)
        {
            RegisterPropertiesWatcher(propertiesToWatch);

            foreach (T obj in objs)
                obj.PropertyChanged += PropertyChangedHandler;
        }


        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (_WatchedProperties.ContainsKey(e.PropertyName))
            {
                PropertyChanged?.Invoke(sender, e);
            }
        }

        protected void RegisterPropertiesWatcher(Expression<Func<T, object>> propertiesToWatch)
        {
            _WatchedProperties = new Dictionary<string, PropertyChangedEventHandler>();

            if (propertiesToWatch == null)
            {
                _WatchedProperties = typeof(T).GetProperties().Select(p => p.Name)
                    .ToDictionary(p => p, p => (PropertyChangedEventHandler)null);
            }
            else if (propertiesToWatch.Body is MemberExpression)
            {
                _WatchedProperties.Add(((MemberExpression)(propertiesToWatch.Body)).Member.Name, null);
            }
            else if (propertiesToWatch.Body is UnaryExpression)
            {
                _WatchedProperties.Add(((MemberExpression)(((UnaryExpression)(propertiesToWatch.Body)).Operand)).Member.Name, null);
            }
            else if (propertiesToWatch.Body.NodeType == ExpressionType.New)
            {
                foreach (var argument in ((NewExpression)propertiesToWatch.Body).Arguments)
                {
                    if (argument is MemberExpression)
                    {
                        _WatchedProperties.Add(((MemberExpression)argument).Member.Name, null);
                    }
                    else
                    {
                        throw new SyntaxErrorException();
                    }
                }
            }
            else
            {
                throw new SyntaxErrorException();
            }
        }
    }
}
