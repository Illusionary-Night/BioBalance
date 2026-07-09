using UnityEngine;


//   原本是存Creature，感覺不應該存Creautre 應該存UUID
public class EnemyAttr : StringAttribute
{
    public EnemyAttr(string initialValue) : base(initialValue)
    {
    }
}