using System;
using System.Collections;
using System.Collections.Generic;

namespace ScratchFramework
{
    public interface IBindable<T>
    {
        public delegate void ValueChangedHandler(T oldValue, T newValue);
    }

    public class BindableProperty<T> : IBindable<T>
    {
        public IBindable<T>.ValueChangedHandler OnValueChanged;
        private object _value;


        public BindableProperty()
        {
            //属性默认值
            if (typeof(T) == typeof(string))
            {
                _value = string.Empty;
            }
            else
            {
                _value = default;
            }
        }

        public T Value
        {
            get => (T)_value;
            set
            {
                if (!Equals(_value, value))
                {
                    object old = _value;
                    _value = value;
                    ValueChanged((T)old, value);
                }
            }
        }

        private void ValueChanged(T oldValue, T newValue)
        {
            if (OnValueChanged != null)
            {
                OnValueChanged(oldValue, newValue);
            }
        }

        public override string ToString()
        {
            return (Value != null ? Value.ToString() : "null");
        }
    }

    public class BindableList<TV> : IBindable<TV>, IEnumerable<TV>
    {
        public IBindable<KeyValuePair<int, TV>>.ValueChangedHandler OnValueChanged;

        private List<TV> _list = new List<TV>();

        public event EventHandler InitListHandler;
        public event EventHandler<KeyValuePair<int, TV>> AddHandler;
        public event EventHandler<KeyValuePair<int, TV>> RemoveHandler;
        public event EventHandler ClearHandler;

        public int Count
        {
            get { return _list.Count; }
        }

        public void InitList(TV[] list) => InitList((IEnumerable<TV>)list);
        public void InitList(List<TV> list) => InitList((IEnumerable<TV>)list);

        public void InitList(IEnumerable<TV> list)
        {
            _list = new List<TV>(list);
            InitListHandler?.Invoke(this, null);
        }

        public TV this[int index]
        {
            get { return _list[index]; }
            set
            {
                if (!Equals(_list[index], value))
                {
                    TV cache = _list[index];
                    _list[index] = value;
                    OnValueChanged?.Invoke(new KeyValuePair<int, TV>(index, cache), new KeyValuePair<int, TV>(index, value));
                }
            }
        }

        public void Insert(int index, TV value)
        {
            try
            {
                _list.Insert(index, value);
                AddHandler?.Invoke(this, new KeyValuePair<int, TV>(index, value));
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public void Add(TV value)
        {
            try
            {
                _list.Add(value);
                AddHandler?.Invoke(this, new KeyValuePair<int, TV>(_list.Count - 1, value));
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public void RemoveAt(int index)
        {
            try
            {
                TV value = _list[index];
                _list.RemoveAt(index);
                RemoveHandler?.Invoke(this, new KeyValuePair<int, TV>(index, value));
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public bool Remove(TV val)
        {
            bool res = false;
            try
            {
                int index = _list.FindIndex(v => v.Equals(val));
                res = _list.Remove(val);
                RemoveHandler?.Invoke(this, new KeyValuePair<int, TV>(index, val));
                return res;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public void Clear()
        {
            _list.Clear();
            ClearHandler?.Invoke(this, null);
        }

        /// <summary> 浅层副本 </summary>
        public List<TV> Clone() => new List<TV>(_list.ToArray());

        /// <summary> 添加值(字典或可绑字典) </summary>
        public void AddValue(IEnumerable<TV> ie)
        {
            foreach (TV val in ie) Add(val);
        }

        public IEnumerator<TV> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }

    public class BindableDictionary<TK, TV> : IBindable<KeyValuePair<TK, TV>>, IEnumerable<KeyValuePair<TK, TV>>
    {
        public IBindable<KeyValuePair<TK, TV>>.ValueChangedHandler OnValueChanged;

        private Dictionary<TK, TV> _dic = new Dictionary<TK, TV>();

        public event EventHandler InitListHandler;
        public event EventHandler<KeyValuePair<TK, TV>> AddPairHandler;
        public event EventHandler<KeyValuePair<TK, TV>> RemovePairHandler;
        public event EventHandler ClearHandler;

        public int Count
        {
            get { return _dic.Count; }
        }

        public Dictionary<TK, TV>.KeyCollection Keys
        {
            get { return _dic.Keys; }
        }

        public Dictionary<TK, TV>.ValueCollection Values
        {
            get { return _dic.Values; }
        }

        public bool ContainsKey(TK key) => _dic.ContainsKey(key);

        public TV this[TK index]
        {
            get { return _dic[index]; }
            set
            {
                if (!_dic.ContainsKey(index) || !Equals(_dic[index], value))
                {
                    if (!_dic.ContainsKey(index))
                    {
                        Add(index, value);
                    }
                    else
                    {
                        KeyValuePair<TK, TV> cache = new KeyValuePair<TK, TV>(index, _dic[index]);
                        _dic[index] = value;

                        OnValueChanged?.Invoke(cache, new KeyValuePair<TK, TV>(index, value));
                    }
                }
            }
        }

        public void InitList(Dictionary<TK, TV> dic)
        {
            _dic = new Dictionary<TK, TV>(dic);
            InitListHandler?.Invoke(this, null);
        }

        public void Add(TK key, TV value)
        {
            _dic.Add(key, value);
            AddPairHandler?.Invoke(this, new KeyValuePair<TK, TV>(key, value));
        }

        public void Remove(TK key)
        {
            var cache = new KeyValuePair<TK, TV>(key, _dic[key]);
            _dic.Remove(key);
            RemovePairHandler?.Invoke(this, cache);
        }

        public void Clear()
        {
            _dic.Clear();
            ClearHandler?.Invoke(this, null);
        }

        /// <summary> 浅层副本 </summary>
        public Dictionary<TK, TV> Clone() => new Dictionary<TK, TV>(_dic);

        /// <summary> 添加值(字典或可绑字典) </summary>
        public void AddValue(IEnumerable<KeyValuePair<TK, TV>> ie)
        {
            foreach (KeyValuePair<TK, TV> pair in ie)
            {
                if (_dic.ContainsKey(pair.Key))
                {
                    KeyValuePair<TK, TV> cache = new KeyValuePair<TK, TV>(pair.Key, _dic[pair.Key]);
                    _dic[pair.Key] = pair.Value;
                    OnValueChanged?.Invoke(cache, pair);
                }
                else Add(pair.Key, pair.Value);
            }
        }

        public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator() => _dic.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dic.GetEnumerator();
        }
    }
}