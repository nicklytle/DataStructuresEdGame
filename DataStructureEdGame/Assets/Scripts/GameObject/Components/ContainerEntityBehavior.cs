using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * This entity can contain other entities inside of it. Its children can be hidden or revealed.
 */
public class ContainerEntityBehavior : MonoBehaviour {

    private List<Transform> children;
    public Material defaultChildMaterial; // the material for when the children objects are revealed.
    public Material fadedChildMaterial; // The material for when the children objected are hidden

    public bool hidden; // if not Hidden, then Revealed. 

    // Use this for initialization
    void Start () {
        children = new List<Transform>();
	}


    /**
     * Get a component from one of its child
     */ 
    public T GetChildComponent<T>()
    {
        foreach (Transform t in children)
        { 
            if (t.GetComponent<T>() != null)
            {
                return t.GetComponent<T>();
            }
        }
        return default(T);
    }

    public void setHidden(bool h)
    {
        if (h != hidden) {
            hidden = h;
            // if it is hidden, fade out all children.
            foreach (Transform t in children)
            {
                if (t.GetComponent<SpriteRenderer>() != null)
                {
                    if (hidden) { 
                        t.GetComponent<SpriteRenderer>().material = fadedChildMaterial;
                    } else
                    {
                        t.GetComponent<SpriteRenderer>().material = defaultChildMaterial;
                    }
                }
            }
        }
        UpdateChildLinkRendering();
    }

    public void UpdateChildLinkRendering()
    {
        foreach (Transform t in children)
        {
            if (t.GetComponent<LinkBehavior>() != null)
            {
                t.GetComponent<LinkBehavior>().selectable = !hidden;  // you can only be selected if you're not hidden.
                if (hidden)
                {
                    t.GetComponent<SpriteRenderer>().material = fadedChildMaterial;
                }
                else
                {
                    t.GetComponent<SpriteRenderer>().material = defaultChildMaterial;
                }
                t.GetComponent<LinkBehavior>().UpdateRendering();
            }
        }
    }

    public bool isHidden()
    {
        return hidden;
    }

    public void refreshChildList()
    {
        if (children == null)
        {
            children = new List<Transform>();
        } else { 
            children.Clear();
        }
        for (int i = 0; i < transform.childCount; i++)
        {
            children.Add(transform.GetChild(i));
        }
    }
}
