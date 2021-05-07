using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionListenerAir : MonoBehaviour
{
    public bool stopMoving;
    public Directions direction;
    public Cell destination;
    private void OnTriggerEnter(Collider other)
    {
        GetComponent<TacticsAttributes>().TakeDamage(0, false);
        if (other.gameObject.CompareTag("Environment"))
        {
            stopMoving = true;
            destination = GameStateManager.FindCell(
                Mathf.RoundToInt(other.transform.position.x), Mathf.RoundToInt(other.transform.position.z))
                .GetNeighbor(GameStateManager.GetOppositeDirection(direction));            
        } else if (other.gameObject.CompareTag("Cover"))
        {
            stopMoving = true;
            destination = other.transform.parent.GetComponent<Cell>().GetNeighbor(GameStateManager.GetOppositeDirection(direction));
        } else if (other.gameObject.GetComponent<TacticsAttributes>())
        {
            other.gameObject.GetComponent<TacticsAttributes>().TakeDamage(0, true);
            stopMoving = true;
            destination = other.transform.GetComponent<TacticsAttributes>().cell.GetNeighbor(GameStateManager.GetOppositeDirection(direction));
        } 
    }
}
