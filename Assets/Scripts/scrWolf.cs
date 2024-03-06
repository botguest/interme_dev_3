using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scrWolf : MonoBehaviour
{
    public enum WolfState
    {
        GoToGrass,
        Idle,
        Chasing,
        Died
    }
    
    public float hungerDecomposeThreshold;
    public float hungerDeadThreshold;
    public float hungerSearchFoodThreshold;
    public Sprite wolf;
    public Sprite wolfDead;
    public float moveSpeed;
    public bool dead;
    
    private Vector3 moveDirection;
    private bool isMoving = false;
    private float moveStartTime;
    private float moveDuration = 5f; // Duration of movement towards grass

    //idle()
    private float directionChangeInterval = 3f;
    private float timeSinceLastDirectionChange;
    private Vector2 randomDirection;
    //idle()
    
    private WolfState currentState;
    private float hungerness;
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
    
    bool tooCloseToSheep()
    {
        GameObject[] sheepObjects = GameObject.FindGameObjectsWithTag("Sheep");
        GameObject closestSheep = null;
        float closestDistance = 120f; //Change this to adjust the trigger chase distance for sheep. To be swapped later with closestDistanceSheep
        Vector3 currentPosition = transform.position;
        
        foreach (GameObject sheep in sheepObjects)
        {
            Vector3 directionToSheep = sheep.transform.position - currentPosition;
            float d = directionToSheep.sqrMagnitude;
            if (d < closestDistance) //the chase is on
            {
                closestSheep = sheep;
                return true;
            }
        }

        return false;
    }
    #endregion
    //Help
    
    //State Switching
    void ChangeState(WolfState newState)
    {
        currentState = newState;
        OnStateChange(newState);
    }

    void OnStateChange(WolfState newState)
    {
        // Handle any initialization for the new state.
        Debug.Log("Changed to new state: " + newState.ToString());
        
        switch (currentState)
        {
            case WolfState.GoToGrass:
                spriteRenderer.sprite = wolf;
                
                break;
            
            case WolfState.Idle:
                spriteRenderer.sprite = wolf;
                moveSpeed = 1f; //change move speed at state change.
                break;
            
            case WolfState.Chasing:
                spriteRenderer.sprite = wolf;
                moveSpeed = 1.5f; //change move speed at state change.
                break;
            
            case WolfState.Died:
                spriteRenderer.sprite = wolfDead;
                dead = true;
                break;
        }
    }
    
    void gotoGrass()
    {
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
            ChangeState(WolfState.Idle); // Change to the next appropriate state
        }
    }
    
    void idle()
    {
        
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
            ChangeState(WolfState.Chasing);
        }
    }

    void chasing()
    {
        hungerness += Time.deltaTime;
        
        GameObject[] sheepObjects = GameObject.FindGameObjectsWithTag("Sheep");
        GameObject closestSheep = null;
        float closestDistance = Mathf.Infinity; //Change this to adjust the trigger chase distance for sheep. To be swapped later with closestDistanceSheep
        Vector3 currentPosition = transform.position;
        
        foreach (GameObject sheep in sheepObjects)
        {
            Vector3 directionToSheep = sheep.transform.position - currentPosition;
            float d = directionToSheep.sqrMagnitude;
            if (d < closestDistance && !sheep.GetComponent<scrSheep>().dead) //the chase is on
            {
                closestDistance = d;
                closestSheep = sheep;
            }
        }
        
        if (closestSheep != null)
        {
            moveDirection = (closestSheep.transform.position - currentPosition).normalized;
            transform.position += new Vector3(moveDirection.x, moveDirection.y, 0) * moveSpeed * Time.deltaTime;
            CheckForEscapeBoundaries();
            
            // Check for overlap with the grass (assuming both have Collider components)
            if (Vector3.Distance(transform.position, closestSheep.transform.position) < 0.5f) // Adjust the distance as needed
            {
                hungerness = 0;
                ChangeState(WolfState.Idle);
            }
        }
        
        if (hungerness >= hungerDeadThreshold)
        {
            ChangeState(WolfState.Died);
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
        ChangeState(WolfState.GoToGrass);
        
        //idle
        timeSinceLastDirectionChange = directionChangeInterval; 
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case WolfState.GoToGrass:
                gotoGrass();
                break;
            case WolfState.Idle:
                idle();
                break;
            case WolfState.Chasing:
                chasing();
                break;
            case WolfState.Died:
                died();
                break;
        }
    }
}
