using UnityEngine;


//   原本是存Creature，感覺不應該存Creautre 應該存UUID
//  我假設沒有enemy就是空字串--by INNT
public class EnemyAttr : StringAttribute
{
    public EnemyAttr(string initialValue) : base(initialValue)
    {
    }
}