using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkBlockBehavior : MonoBehaviour {

    public Transform referencePlatform; // this is the platform object this link is pointing to.
    public Transform linkArrow; // this is the current arrow that is instantiated 
    public Transform linkArrowPreFab;

	// Use this for initialization
	void Start () {
        linkArrow = null;
        UpdateLinkArrow();
    }
	
    void UpdateLinkArrow()
    {
        if (referencePlatform == null)
        {
            if (linkArrow != null)
            {
                Destroy(linkArrow);
            }
            linkArrow = null;
        } else
        {
            // how far the arrow is in GameUnits and where it will be positioned
            // 1 Game Unit ~= 64 Pixels sprite?
            Bounds linkBounds = GetComponent<SpriteRenderer>().bounds;
            Bounds platBounds = referencePlatform.GetComponent<SpriteRenderer>().bounds; 

            // find the closest points on both bounding boxes to the center point to make the arrow.
            Vector3 betweenPoint = new Vector3((linkBounds.center.x + platBounds.center.x) / 2, 
                (linkBounds.center.y + platBounds.center.y) / 2, 0);
            Debug.Log(betweenPoint);
            Vector3 closestToLink = linkBounds.ClosestPoint(betweenPoint);
            Vector3 closestToPlat = platBounds.ClosestPoint(betweenPoint);
            betweenPoint = (closestToLink + closestToPlat) / 2; // update the between point

            linkArrow = Instantiate(linkArrowPreFab, betweenPoint, Quaternion.identity);
            linkArrow.transform.localScale = new Vector3(Vector3.Distance(closestToLink, closestToPlat), 1, 1);
            // linkArrow.Rotate. = Quaternion.identity.SetFromToRotation(closestToLink, closestToPlat);
            Vector3 diff = closestToPlat - closestToLink;
            float rotationAmount = 0;
            if (diff.y != 0)
            {
                rotationAmount = Mathf.Sin(diff.y / diff.magnitude);
                if (diff.x < 0)
                {
                    linkArrow.transform.localScale = new Vector3(-linkArrow.transform.localScale.x, 
                        linkArrow.transform.localScale.y, linkArrow.transform.localScale.z);
                    rotationAmount *= -1;
                }
            } else if (diff.x < 0)
            {
                linkArrow.transform.localScale = new Vector3(-linkArrow.transform.localScale.x,
                        linkArrow.transform.localScale.y, linkArrow.transform.localScale.z);
            }
            linkArrow.transform.Rotate(new Vector3(0, 0, Mathf.Rad2Deg * rotationAmount ));
        }
    }

	// Update is called once per frame
	void Update () {
		
	}
}
