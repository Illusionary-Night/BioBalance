using UnityEngine;
using UnityEngine.UIElements.Experimental;

//開碰撞體

public class Food : MonoBehaviour , Tickable
{
    public int lifeSpan { get; set; } // 食物的壽命
    public void OnTick()
    {
        lifeSpan--;
        if (lifeSpan <= 0)
        {
            Destroy(this.gameObject);
        }
    }
    public void Eaten()
    {
        Destroy(this.gameObject);
    }
    public void Initialize(int lifespan, Vector2 position)
    {
        lifeSpan = lifespan;
        this.transform.position = position;
    }
}
