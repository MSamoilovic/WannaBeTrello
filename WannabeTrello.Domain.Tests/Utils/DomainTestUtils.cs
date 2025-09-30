using System.Reflection;
using System.Runtime.Serialization;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Events;

namespace WannabeTrello.Domain.Tests.Utils;

public static class DomainTestUtils
{
    
    public static T CreateInstanceWithoutConstructor<T>() where T : class
    {
        return (T)FormatterServices.GetUninitializedObject(typeof(T));
    }
    
    
    public static void SetPrivatePropertyValue<T>(T obj, string propertyName, object value) where T : class
    {
        var property = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        var currentType = typeof(T).BaseType;
        while (currentType != null && property == null)
        {
            property = currentType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            currentType = currentType.BaseType;
        }
        
        if (property != null)
        {
            property.SetValue(obj, value, null);
        }
        else
        {
           
            var field = typeof(T).GetField(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);
            var baseType = typeof(T).BaseType;
            while(baseType != null && field == null)
            {
                field = baseType.GetField(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);
                baseType = baseType.BaseType;
            }
            field?.SetValue(obj, value);
        }
    }
    
    public static void InitializeDomainEvents(AuditableEntity entity)
    {
        var domainEventsList = new List<DomainEvent>();
        SetPrivatePropertyValue(entity, "_domainEvents", domainEventsList);
    }
}