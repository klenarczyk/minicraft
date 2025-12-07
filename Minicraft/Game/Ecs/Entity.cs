namespace Minicraft.Game.Ecs;

public class Entity
{
    public List<object> Components = [];

    public T GetComponent<T>()
    {
        var component = Components.OfType<T>().FirstOrDefault();
        return component ?? throw new Exception($"Component of type {typeof(T)} not found in entity.");
    }

    public void AddComponent(object component)
    {
        Components.Add(component);
    }
}