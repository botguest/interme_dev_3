using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class scrSheep : MonoBehaviour
{
    public enum SheepState
    {
        GoToGrass,
        Idle,
        FindFood,
        Chased,
        Died
    }

    public float hungerDecomposeThreshold;
    public float hungerDeadThreshold;
    public float hungerSearchFoodThreshold;
    public Sprite sheep;
    public Sprite sheepDead;
    public float moveSpeed;
    public float closestDistanceSheep; //chase trigger distance of sheep.
    public float closestDistanceWolf; //chase trigger distance of wolf.
    public bool dead;
    
    private Vector3 moveDirection;
    private bool isMoving = false;
    private float moveDuration = 5f; // The sheep moves for 5 seconds
    private float moveStartTime;
    
    //idle()
    private float directionChangeInterval = 3f;
    private float timeSinceLastDirectionChange;
    private Vector2 randomDirection;
    //idle()
    
    private SheepState currentState;
    private float hungerness; //increase in seconds
    private SpriteRenderer spriteRenderer;
    
    //Helper
    #region Helper
    
    public void ChangeScale(float newXScale, float newYScale)
    {
        // Set the new scale of the GameObject
        // We keep the original Z scale unchanged
        transform.localScale = new Vector3(newXScale, newYScale, transform.localScale.z);
    }
    
    void CheckForBoundaries()
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        Vector3 newDirection = randomDirection;

        if (screenPoint.x < 0.05f)
        {
            newDirection = new Vector2(1, Random.Range(-1f, 1f)); // Move right
        }
        else if (screenPoint.x > 0.95f)
        {
            newDirection = new Vector2(-1, Random.Range(-1f, 1f)); // Move left
        }

        if (screenPoint.y < 0.05f)
        {
            newDirection = new Vector2(Random.Range(-1f, 1f), 1); // Move up
        }
        else if (screenPoint.y > 0.95f)
        {
            newDirection = new Vector2(Random.Range(-1f, 1f), -1); // Move down
        }

        randomDirection = newDirection.normalized;
        timeSinceLastDirectionChange = 0; // Reset direction change timer to immediately adjust direction

        // Clamp position as a fail-safe to prevent going out of bounds
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, Camera.main.ViewportToWorldPoint(new Vector3(0.05f, 0, 0)).x, Camera.main.ViewportToWorldPoint(new Vector3(0.95f, 0, 0)).x);
        position.y = Mathf.Clamp(position.y, Camera.main.ViewportToWorldPoint(new Vector3(0, 0.05f, 0)).y, Camera.main.ViewportToWorldPoint(new Vector3(0, 0.95f, 0)).y);
        transform.position = position;
    }

    void CheckForEscapeBoundaries()
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        Vector3 newDirection = moveDirection;

        if (screenPoint.x < 0.05f)
        {
            newDirection = new Vector2(1, Random.Range(-1f, 1f)); // Move right
        }
        else if (screenPoint.x > 0.95f)
        {
            newDirection = new Vector2(-1, Random.Range(-1f, 1f)); // Move left
        }

        if (screenPoint.y < 0.05f)
        {
            newDirection = new Vector2(Random.Range(-1f, 1f), 1); // Move up
        }
        else if (screenPoint.y > 0.95f)
        {
            newDirection = new Vector2(Random.Range(-1f, 1f), -1); // Move down
        }

        moveDirection = newDirection.normalized;
        timeSinceLastDirectionChange = 0; // Reset direction change timer to immediately adjust direction

        // Clamp position as a fail-safe to prevent going out of bounds
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, Camera.main.ViewportToWorldPoint(new Vector3(0.05f, 0, 0)).x, Camera.main.ViewportToWorldPoint(new Vector3(0.95f, 0, 0)).x);
        position.y = Mathf.Clamp(position.y, Camera.main.ViewportToWorldPoint(new Vector3(0, 0.05f, 0)).y, Camera.main.ViewportToWorldPoint(new Vector3(0, 0.95f, 0)).y);
        transform.position = position;
    }
    
    bool tooCloseToWolf()
    {
        GameObject[] wolfObjects = GameObject.FindGameObjectsWithTag("Wolf");
        GameObject closestWolf = null;
        float closestDistance = 12f; //Change this to adjust the trigger chase distance for sheep. To be swapped later with closestDistanceSheep
        Vector3 currentPosition = transform.position;
        
        foreach (GameObject wolf in wolfObjects)
        {
            Vector3 directionToWolf = wolf.transform.position - currentPosition;
            float d = directionToWolf.sqrMagnitude;
            if (d < closestDistance && !wolf.GetComponent<scrWolf>().dead) //the chase is on
            {
                closestWolf = wolf;
                return true;
            }
        }

        return false;
    }
    #endregion
    //Helper
    
    //State Switching
    void ChangeState(SheepState newState)
    {
        currentState = newState;
        OnStateChange(newState);
    }

    void OnStateChange(SheepState newState)
    {
        // Handle any initialization for the new state.
        Debug.Log("Changed to new state: " + newState.ToString());
        // This method can be expanded.
        switch (currentState)
        {
            case SheepState.GoToGrass:
                spriteRenderer.sprite = sheep;
                
                break;
            
            case SheepState.Idle:
                spriteRenderer.sprite = sheep;
                moveSpeed = 0.3f; //change move speed at state change.
                break;
            
            case SheepState.FindFood:
                spriteRenderer.sprite = sheep;
                moveSpeed = 0.7f; //change move speed at state change.
                break;
            
            case SheepState.Chased:
                spriteRenderer.sprite = sheep;
                moveSpeed = 0.9f; //change move speed at state change.
                break;
            
            case SheepState.Died:
                spriteRenderer.sprite = sheepDead;
                dead = true;
                break;
        }
    }
    //State Switching

    void gotoGrass()
    {
        //If too close, go to chase.
        if (tooCloseToWolf())
        {
            ChangeState(SheepState.Chased);
        }
        //If too close, go to chase.
        
        if (!isMoving)
        {
            // Calculate the target position at the center of the camera's view
            Vector3 targetPosition = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Camera.main.nearClipPlane));
            targetPosition.z = transform.position.z; // Ensure target position has the same z value as the sheep

            // Calculate the direction vector from the sheep's current position to the target position
            moveDirection = (targetPosition - transform.position).normalized;
            

            isMoving = true;
            moveStartTime = Time.time;
        }
        
        if (isMoving && (Time.time - moveStartTime) <= moveDuration)
        {
            // Move the sheep along the direction vector at the specified speed
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
        else if (isMoving && (Time.time - moveStartTime) > moveDuration)
        {
            // Stop moving after 5 seconds
            isMoving = false;
            ChangeState(SheepState.Idle); // Change to the next appropriate state
        }
    }

    void idle()
    {
        //If too close, go to chase.
        if (tooCloseToWolf())
        {
            ChangeState(SheepState.Chased);
        }
        //If too close, go to chase.
        
        hungerness += Time.deltaTime;
        timeSinceLastDirectionChange += Time.deltaTime;
        
        if (timeSinceLastDirectionChange >= directionChangeInterval)
        {
            // Generate a new random direction
            randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            timeSinceLastDirectionChange = 0f;
        }
        
        // Move in the new direction
        transform.position += new Vector3(randomDirection.x, randomDirection.y, 0) * moveSpeed * Time.deltaTime;
        
        // Check for boundaries and change direction if near
        CheckForBoundaries();
        
        if (hungerness >= hungerSearchFoodThreshold)
        {
            ChangeState(SheepState.FindFood);
        }
    }

    void findFood()
    {
        //If too close, go to chase.
        if (tooCloseToWolf())
        {
            ChangeState(SheepState.Chased);
        }
        //If too close, go to chase.
        
        GameObject[] grassObjects = GameObject.FindGameObjectsWithTag("Grass"); // Ensure your prefGrass prefabs are tagged with "Grass"
        GameObject closestGrass = null;
        float closestDistance = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        
        hungerness += Time.deltaTime;

        // Find the closest grass
        foreach (GameObject grass in grassObjects)
        {
            Vector3 directionToGrass = grass.transform.position - currentPosition;
            float d = directionToGrass.sqrMagnitude;
            if (d < closestDistance)
            {
                closestDistance = d;
                closestGrass = grass;
            }
        }

        // Move towards the closest grass if it exists
        if (closestGrass != null)
        {
            moveDirection = (closestGrass.transform.position - currentPosition).normalized;
            transform.position += new Vector3(moveDirection.x, moveDirection.y, 0) * moveSpeed * Time.deltaTime;

            // Check for overlap with the grass (assuming both have Collider components)
            if (Vector3.Distance(transform.position, closestGrass.transform.position) < 0.5f) // Adjust the distance as needed
            {
                // Set the eaten flag on the grass
                closestGrass.GetComponent<scrGrass>().eaten = true;
                hungerness = 0;
                ChangeState(SheepState.Idle);
            }
        }

        if (hungerness >= hungerDeadThreshold)
        {
            ChangeState(SheepState.Died);
        }
    }

    void chased()
    {
        //check whether hungry enough
        if (hungerness <= hungerSearchFoodThreshold)
        {
            if (!tooCloseToWolf())
            {
                ChangeState(SheepState.Idle);
            }
            
            hungerness += Time.deltaTime;
        } else if (hungerness >= hungerSearchFoodThreshold)
        {
            if (hungerness >= hungerDeadThreshold)
            {
                ChangeState(SheepState.Died);
            }

            if (!tooCloseToWolf())
            {
                ChangeState(SheepState.FindFood);
            }
            hungerness += Time.deltaTime;
            //might be of use later.
        }
        //check whether hungry enough
        
        //now that it's too close, we go into chase.
        GameObject[] wolfObjects = GameObject.FindGameObjectsWithTag("Wolf");
        GameObject closestWolf = null;
        float closestDistance = Mathf.Infinity; 
        Vector3 currentPosition = transform.position;
        
        foreach (GameObject wolf in wolfObjects)
        {
            Vector3 directionToWolf = wolf.transform.position - currentPosition;
            float d = directionToWolf.sqrMagnitude;
            if (d < closestDistance) 
            {
                closestDistance = d;
                closestWolf = wolf;
            }
        }

        if (closestWolf != null)
        {
            moveDirection = (closestWolf.transform.position - currentPosition).normalized;
            transform.position += -(new Vector3(moveDirection.x, moveDirection.y, 0) * moveSpeed * Time.deltaTime);
            CheckForEscapeBoundaries();
            
            // Check for overlap with the grass (assuming both have Collider components)
            if (Vector3.Distance(transform.position, closestWolf.transform.position) < 0.5f) // Adjust the distance as needed
            {
                // Set the eaten flag on the sheep
                hungerness = hungerDeadThreshold;
                ChangeState(SheepState.Died);
            }
        }
    }

    void died()
    {
        hungerness += Time.deltaTime;
        if (hungerness >= hungerDecomposeThreshold)
        {
            Destroy(gameObject);
        }
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ChangeState(SheepState.GoToGrass);
        
        //idle
        timeSinceLastDirectionChange = directionChangeInterval; 
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case SheepState.GoToGrass:
                gotoGrass();
                break;
            case SheepState.Idle:
                idle();
                break;
            case SheepState.FindFood:
                findFood();
                break;
            case SheepState.Chased:
                chased();
                break;
            case SheepState.Died:
                died();
                break;
        }
    }
}
