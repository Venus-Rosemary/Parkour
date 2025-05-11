using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleChange : MonoBehaviour
{
    public GameObject obstacleO;
    public GameObject coinBigO;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetObstacleChange()
    {
        if (obstacleO != null)
        {
            obstacleO.SetActive(false);
        }
        if (coinBigO != null && obstacleO != null)
        {
            coinBigO.SetActive(true);
        }
    }

    public void SetObstacleRestore()
    {
        if (obstacleO != null)
        {
            obstacleO.SetActive(true);
        }
        if (coinBigO != null)
        {
            coinBigO.SetActive(false);
        }
    }

    public void SetObstacleActive(bool obstacle)
    {
        if (obstacleO != null)
        {
            Destroy(obstacleO);
        }
    }
}
