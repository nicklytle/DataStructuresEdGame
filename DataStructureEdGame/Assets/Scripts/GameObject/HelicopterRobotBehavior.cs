using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * The special helicopter link block
 */
public class HelicopterRobotBehavior : MonoBehaviour
{
    public GameController gameController; 
    public Vector3 targetLocation;
    public float flySpeed;

	void Start () {
        GetComponent<ContainerEntityBehavior>().refreshChildList();
    }

    // Update is called once per frame
    void Update() {
        float distAway = Vector3.Distance(transform.position, targetLocation);
        LinkBehavior childLink = getChildLink();
        if (childLink != null)
        {
            if (distAway > 0.1f)
            {
                Vector3 moveDir = (targetLocation - transform.position).normalized;
                // move, lerping based on the distance away
                transform.Translate(Vector3.Lerp(new Vector3(), (moveDir * flySpeed * Time.deltaTime), distAway / 12f));
                if (childLink != null && childLink.connectableEntity != null)
                {
                    childLink.GetComponent<LinkBehavior>().setRenderArrow(false);
                    childLink.GetComponent<LinkBehavior>().UpdateRendering(); // refresh its position as it moves
                }
            }
            else
            {
                childLink.setRenderArrow(true);
            }
        }
    }

    public void MoveAboveLinkedPlatform()
    {
        LinkBehavior childLink = getChildLink();
        if (childLink.connectableEntity != null)
        {
            targetLocation = childLink.connectableEntity.transform.position + (new Vector3(0, 3, 0));
        }
    }

    public LinkBehavior getChildLink()
    {
        LinkBehavior lb = GetComponent<ContainerEntityBehavior>().GetChildComponent<LinkBehavior>();
        if (lb == null)
        {
            GetComponent<ContainerEntityBehavior>().refreshChildList();
            lb = GetComponent<ContainerEntityBehavior>().GetChildComponent<LinkBehavior>();
        }
        return lb;
    }

}
