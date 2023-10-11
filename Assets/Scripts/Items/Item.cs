using UnityEngine;

public class Item : MonoBehaviour
{
    //Update to update Logic of the Item
    public virtual void Update()
    {

    }

    //On Pick Up happens when the Items Collides with the Player, Item Logic Here
    public virtual void OnPickUp(Collider2D collision)
    {

    }

    //On Trigger Enter 2D gives the start to the Logic of the Item, it also destroys it
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            OnPickUp(collision);
        }
    }
}