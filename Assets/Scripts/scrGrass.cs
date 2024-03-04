using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scrGrass : MonoBehaviour
{
    
    public enum GrassState
    {
        Growing,
        Dying,
        Dead
    }

    public float matureAge;
    public float deadAge;
    public float decomposeAge;
    public bool eaten;
    public Sprite grass;
    public Sprite grassDead;
    
    
    private GrassState currentState;
    private float age; // the age of the grass.
    private SpriteRenderer spriteRenderer;
    
    //Helper
    public void ChangeScale(float newXScale, float newYScale)
    {
        // Set the new scale of the GameObject
        // We keep the original Z scale unchanged
        transform.localScale = new Vector3(newXScale, newYScale, transform.localScale.z);
    }
    //Helper
    
    //State Switching
    void ChangeState(GrassState newState)
    {
        currentState = newState;
        OnStateChange(newState);
    }

    void OnStateChange(GrassState newState)
    {
        // Handle any initialization for the new state.
        Debug.Log("Changed to new state: " + newState.ToString());
        // This method can be expanded.
        switch (currentState)
        {
            case GrassState.Growing:
                spriteRenderer.sprite = grass;
                
                ChangeScale(1f, 1f);
                break;
            
            case GrassState.Dying:
                spriteRenderer.sprite = grass;
                
                ChangeScale(2f, 2f);
                break;
            
            case GrassState.Dead:
                spriteRenderer.sprite = grassDead;
                
                ChangeScale(1.5f, 1.5f);
                break;
        }
    }
    //State Switching

    void growing()
    {
        age += Time.deltaTime;
        if (eaten)
        {
            Destroy(gameObject);
        }
        
        if (age >= matureAge)
        {
            ChangeState(GrassState.Dying);
        }
        
    }

    void dying()
    {
        age += Time.deltaTime;
        if (eaten)
        {
            ChangeState(GrassState.Growing);
        }

        if (age >= deadAge)
        {
            ChangeState(GrassState.Dead);
        }
    }

    void dead()
    {
        age += Time.deltaTime;
        if (age >= decomposeAge)
        {
            Destroy(gameObject);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        eaten = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
        ChangeState(GrassState.Growing);
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case GrassState.Growing:
                growing();
                break;
            
            case GrassState.Dying:
                dying();
                break;
            
            case GrassState.Dead:
                dead();
                break;
        }
    }
}
