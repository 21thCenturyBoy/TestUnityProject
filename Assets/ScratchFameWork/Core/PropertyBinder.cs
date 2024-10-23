using System;
using System.Collections.Generic;
using System.Reflection;

namespace ScratchFramework
{
    /// <summary>
/// 属性绑定器
/// </summary>
/// <typeparam name="T"></typeparam>
public class PropertyBinder<T> where T : ScratchVMData
{
    private delegate void BindHandler(T viewmodel);
    private delegate void UnBindHandler(T viewmodel);

    //上下文改变时调用
    private readonly List<BindHandler> _binders = new List<BindHandler>();
    private readonly List<UnBindHandler> _unbinders = new List<UnBindHandler>();

    public void AddList<TV>(string name, IBindable<KeyValuePair<int, TV>>.ValueChangedHandler valueChangedHandler)
    {
        var fieldInfo = typeof(T).GetField(name, BindingFlags.Instance | BindingFlags.Public);
        if (fieldInfo == null)
        {
            throw new Exception($"Unable to find bindableproperty field '{typeof(T).Name}.{name}'");
        }
        _binders.Add(viewmodel =>
        {
            GetListValue<TV>(name, viewmodel, fieldInfo).OnValueChanged += valueChangedHandler;
        });
        _unbinders.Add(viewModel =>
        {
            GetListValue<TV>(name, viewModel, fieldInfo).OnValueChanged -= valueChangedHandler;
        });
    }

    public void AddDict<TK, TV>(string name, IBindable<KeyValuePair<TK, TV>>.ValueChangedHandler valueChangedHandler)
    {
        var fieldInfo = typeof(T).GetField(name, BindingFlags.Instance | BindingFlags.Public);
        if (fieldInfo == null)
        {
            throw new Exception($"Unable to find bindableproperty field '{typeof(T).Name}.{name}'");
        }
        _binders.Add(viewmodel =>
        {
            GetDictoraryValue<TK, TV>(name, viewmodel, fieldInfo).OnValueChanged += valueChangedHandler;
        });
        _unbinders.Add(viewModel =>
        {
            GetDictoraryValue<TK, TV>(name, viewModel, fieldInfo).OnValueChanged -= valueChangedHandler;
        });
    }
    public void Add<TProperty>(string name, IBindable<TProperty>.ValueChangedHandler valueChangedHandler)
    {
        var fieldInfo = typeof(T).GetField(name, BindingFlags.Instance | BindingFlags.Public);
        if (fieldInfo == null)
        {
            throw new Exception($"Unable to find bindableproperty field '{typeof(T).Name}.{name}'");
        }

        _binders.Add(viewmodel =>
        {
            GetPropertyValue<TProperty>(name, viewmodel, fieldInfo).OnValueChanged += valueChangedHandler;
        });

        _unbinders.Add(viewModel =>
        {
            GetPropertyValue<TProperty>(name, viewModel, fieldInfo).OnValueChanged -= valueChangedHandler;
        });

    }
    private BindableProperty<TProperty> GetPropertyValue<TProperty>(string name, T viewModel, FieldInfo fieldInfo)
    {
        var value = fieldInfo.GetValue(viewModel);
        BindableProperty<TProperty> bindableProperty = value as BindableProperty<TProperty>;
        if (bindableProperty == null)
        {
            throw new Exception($"Illegal bindableproperty field '{typeof(T).Name}.{name}' ");
        }

        return bindableProperty;
    }
    private BindableDictionary<TK, TV> GetDictoraryValue<TK, TV>(string name, T viewModel, FieldInfo fieldInfo)
    {
        var value = fieldInfo.GetValue(viewModel);
        BindableDictionary<TK, TV> bindableProperty = value as BindableDictionary<TK, TV>;
        if (bindableProperty == null)
        {
            throw new Exception($"Illegal bindableproperty field '{typeof(T).Name}.{name}' ");
        }
        return bindableProperty;
    }
    private BindableList<TV> GetListValue<TV>(string name, T viewModel, FieldInfo fieldInfo)
    {
        var value = fieldInfo.GetValue(viewModel);
        BindableList<TV> bindableProperty = value as BindableList<TV>;
        if (bindableProperty == null)
        {
            throw new Exception($"Illegal bindableproperty field '{typeof(T).Name}.{name}' ");
        }
        return bindableProperty;
    }

    public void Bind(T viewmodel)
    {
        if (viewmodel != null)
        {
            for (int i = 0; i < _binders.Count; i++)
            {
                _binders[i](viewmodel);
            }
        }
    }

    public void Unbind(T viewmodel)
    {
        if (viewmodel != null)
        {
            for (int i = 0; i < _unbinders.Count; i++)
            {
                _unbinders[i](viewmodel);
            }
        }
    }

}
}

