using System.Collections;
using Actors;
using Enemy;
using Infrastructure.Data.ScriptableObjects;
using Infrastructure.Services.LevelService;
using Scripts.Infrastructure;
using UnityEngine;
using Zenject;

namespace Logic.AI.States
{
    public class AiPatrolState : IState
    {
        private readonly AiData _aiData;
        private readonly Actor _aiOwner;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly AiStateMachine _stateMachine;

        private Coroutine _patrolCoroutine;
        private readonly IAIMovement _movement;

        private Vector3 _patrolPosition;

        public AiPatrolState(AiStateMachine stateMachine, DiContainer diContainer, Actor aiOwner, ICoroutineRunner coroutineRunner)
        {
            _aiOwner = aiOwner;
            _coroutineRunner = coroutineRunner;
            _stateMachine = stateMachine;

            ILevelService levelService = diContainer.Resolve<ILevelService>();

            _aiData = levelService.AIParametersData;

            _movement = _aiOwner.GetComponent<IAIMovement>();
        }

        public void Enter()
        {
            _patrolCoroutine = _coroutineRunner.StartCoroutine(Patrol());
        }

        private IEnumerator Patrol()
        {
            float timePatrol = _aiData.GetTimePatrol();
            _patrolPosition = GetRandomPoint();
            
            while (timePatrol >= 0.0f)
            {
                if (CheckGoldTravel())
                {
                    _stateMachine.Enter<AiGoToNextIslandState>();
                }
                
                timePatrol -= Time.deltaTime;

                if (_movement.IsDisableMovement == false)
                {
                    if (Vector3.Distance(_aiOwner.ActorTransform.position, _patrolPosition) <= _aiOwner.ApproachDistance)
                    {
                        _patrolPosition = GetRandomPoint();
                    }
                    else
                    {
                        _movement.SetDestination(_patrolPosition);
                    }
                }
                
                if(_aiOwner.CurrentIsland.GetCountGoldLoots() > 0)
                    _stateMachine.Enter<AiCoinSearch>();
                
                yield return null;
            }
            
            _stateMachine.Enter<AiTriggerGoldSearchState>();
            _patrolCoroutine = null;
        }

        public void Exit()
        {
            if (_patrolCoroutine != null) 
                _coroutineRunner.StopCoroutine(_patrolCoroutine);
        }

        private Vector3 GetRandomPoint()
        {
            return _aiOwner.CurrentIsland.GetRandomPointInIsland();
        }

        private bool CheckGoldTravel()
        {
            return _aiOwner.GoldService.CurrentCount >= _aiOwner.CurrentIsland.CostDelivery;
        }
    }
}