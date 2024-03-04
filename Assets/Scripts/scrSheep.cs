using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public float hungerDeadThreshold;
    public float hungerSearchFoodThreshold;
    public Sprite sheep;
    public Sprite sheepDead;
    public float moveSpeed;
    
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
    public void ChangeScale(float newXScale, float newYScale)
    {
        // Set the new scale of the GameObject
        // We keep the original Z scale unchanged
        transform.localScale = new Vector3(newXScale, newYScale, transform.localScale.z);
    }
    
    void CheckForBoundaries()
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        bool isNearBoundary = false;

        if (screenPoint.x < 0.1f)
        {
            isNearBoundary = true;
            randomDirection = new Vector2(Mathf.Abs(randomDirection.x), randomDirection.y);
        }
        else if (screenPoint.x > 0.9f)
        {
            isNearBoundary = true;
            randomDirection = new Vector2(-Mathf.Abs(randomDirection.x), randomDirection.y);
        }

        if (screenPoint.y < 0.1f)
        {
            isNearBoundary = true;
            randomDirection = new Vector2(randomDirection.x, Mathf.Abs(randomDirection.y));
        }
        else if (screenPoint.y > 0.9f)
        {
            isNearBoundary = true;
            randomDirection = new Vector2(randomDirection.x, -Mathf.Abs(randomDirection.y));
        }

        if (isNearBoundary)
        {
            timeSinceLastDirectionChange = directionChangeInterval; // Reset direction change timer
        }
    }
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
                moveSpeed = 0.5f;
                break;
            
            case SheepState.FindFood:
                spriteRenderer.sprite = sheep;
                moveSpeed = 1f;
                break;
            
            case SheepState.Chased:
                spriteRenderer.sprite = sheep;
                
                break;
            
            case SheepState.Died:
                spriteRenderer.sprite = sheepDead;
                
                break;
        }
    }
    //State Switching

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
            ChangeState(SheepState.Idle); // Change to the next appropriate state
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
            ChangeState(SheepState.FindFood);
        }
    }

    void findFood()
    {
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
        
    }

    void died()
    {
        
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
