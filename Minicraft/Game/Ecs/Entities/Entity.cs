using Minicraft.Game.Ecs.Components;

namespace Minicraft.Game.Ecs.Entities;

/// <summary>
/// A general-purpose container for components.
/// Behavior is defined by the attached <see cref="IComponent"/> data.
/// </summary>
public class Entity
{
    private readonly List<IComponent> _components = [];
    private readonly Dictionary<Type, IComponent> _componentCache = new();

    public IReadOnlyList<IComponent> Components => _components;

    /// <summary>
    /// Adds a new component to the entity.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if a component of this type already exists.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the component is null.</exception>
    public void AddComponent<T>(T component) where T : IComponent
    {
        if (component == null)
            throw new ArgumentNullException(nameof(component));

        var type = component.GetType();
        if (_componentCache.ContainsKey(type))
            throw new InvalidOperationException($"Entity already has a component of type {type.Name}");

        _components.Add(component);
        _componentCache[type] = component;
    }

    /// <summary>
    /// Removes the component of type <typeparamref name="T"/> if it exists.
    /// </summary>
    public void RemoveComponent<T>() where T : IComponent
    {
        var type = typeof(T);
        if (_componentCache.Remove(type, out var component))
        {
            _components.Remove(component);
        }
    }

    /// <summary>
    /// Retrieves the component of type <typeparamref name="T"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the component is missing.</exception>
    public T GetComponent<T>() where T : IComponent
    {
        if (_componentCache.TryGetValue(typeof(T), out var component))
            return (T)component;

        throw new InvalidOperationException($"Entity does not have component {typeof(T).Name}");
    }

    public bool TryGetComponent<T>(out T? component) where T : IComponent
    {
        if (_componentCache.TryGetValue(typeof(T), out var comp))
        {
            component = (T)comp;
            return true;
        }

        component = default;
        return false;
    }

    public bool HasComponent<T>() where T : IComponent
    {
        return _componentCache.ContainsKey(typeof(T));
    }
}