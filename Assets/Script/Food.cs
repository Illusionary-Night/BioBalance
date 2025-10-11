using UnityEngine;
using UnityEngine.UIElements.Experimental;

//�}�I����

public class Food : MonoBehaviour , Tickable
{
    public int lifeSpan { get; set; } // �������ةR
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
