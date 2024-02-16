using Blazorise;
using CBC.Shared;
using ConcurrentLinkedListQueue;
using static VideoChatHub;

namespace CBC.Server
{
    internal class TalkersQueues
    {
        int AgeMinimum;
        internal TalkersQueues(int AgeMin, int AgeMax)
        {
            AgeMinimum = AgeMin;
            var Fields = AgeMax - AgeMin;
            Males = new ConcurrentLinkedListQueue<InQueueStatus>[Fields];
            Females = new ConcurrentLinkedListQueue<InQueueStatus>[Fields];
            for (int i = 0; i < Fields; i++)
            {
                Males[i] = new ConcurrentLinkedListQueue<InQueueStatus>();
                Females[i] = new ConcurrentLinkedListQueue<InQueueStatus>();
            }
        }

        internal void Push(int Age, bool Female, InQueueStatus user)
        {
            if(Female)
            {
                Females[Age - AgeMinimum].Enqueue(user);
                return;
            }
            Males[Age - AgeMinimum].Enqueue(user);
            //Females[Age].Enqueue(user);
        }

        internal InQueueStatus GetId(UserPreferences requirements, InQueueStatus qStatus, string ConnID)
        {
            int avr = ((requirements.MinAge + requirements.MaxAge) / 2) - AgeMinimum;
            var user = qStatus.user;
            IEnumerable<string> NotAcceptable = 
                qStatus.recent
            .Select(item => item.TryGetTarget(out var status) ? status.preferences.ConnectionId : null).Concat(
                qStatus.disliked
            .Select(item => item.TryGetTarget(out var status) ? status.preferences.ConnectionId : null)
                ).Where(item => item != null)!;

            for (int i = requirements.MaxAge - AgeMinimum; i > requirements.MinAge - AgeMinimum; )
            {
                --i;
                //if (requirements.AcceptMale)
                //{
                //    if (r < requirements.MaxAge && Males[r].GetFirstUserWithCondition(ueueUser => ueueUser.MinAge > user.Age, ref ConnID))
                //    {
                //        break;
                //    }
                //    if (l >= 0 && Males[l].GetFirstUserWithCondition(ueueUser => ueueUser.MinAge > user.Age, ref ConnID))
                //    {
                //        break;
                //    }
                //}
                //if (requirements.AcceptFemale)
                //{
                //    if (r < requirements.MaxAge && Females[r].GetFirstUserWithCondition(ueueUser => ueueUser.MinAge > user.Age, ref ConnID))
                //    {
                //        break;
                //    }
                //    if (l >= 0 && Females[l].GetFirstUserWithCondition(ueueUser => ueueUser.MinAge > user.Age, ref ConnID))
                //    {
                //        break;
                //    }
                //}
                if (requirements.AcceptFemale){
                    var gotVal = Females[i].GetFirstUserWithCondition(
                        ueueUser => MatchCheck(ueueUser, user, NotAcceptable, ConnID));
                    if (gotVal is object)
                    {
                        return gotVal;
                    }
                }
                if (requirements.AcceptMale) {
                    var gotVal = Males[i].GetFirstUserWithCondition(
                        ueueUser => MatchCheck(ueueUser, user, NotAcceptable, ConnID));
                    if (gotVal is object)
                    {
                        return gotVal;
                    }
                }
            }
            return null!;
        }

        private bool MatchCheck(InQueueStatus ueueUser, QueueUser user, IEnumerable<string> NotAcceptable, string ConnId)
        {
            return ueueUser.preferences.MinAge <= user.Age &&
            ueueUser.preferences.MaxAge >= user.Age &&
            (user.IsFemale ? ueueUser.preferences.AcceptFemale : ueueUser.preferences.AcceptMale) &&
            ueueUser.preferences.ConnectionId != ConnId &&

           !NotAcceptable.Contains(ueueUser.preferences.ConnectionId) &&
           (ueueUser.recent.SearchFromMostRecent(recent => MatchesId(recent, ConnId)) is null) &&
           (ueueUser.disliked.SearchFromMostRecent(disliked => MatchesId(disliked, ConnId)) is null);
        }

        private bool MatchesId(WeakReference<InQueueStatus> weakReference, string ID)
        {
            if (weakReference.TryGetTarget(out var status))
            {
                if (status.preferences.ConnectionId == ID)
                {
                    return true;
                }
            }
            return false;
        }

        internal bool RemoveUser(QueueUser user, string ConnID)
        {
            if (user.IsFemale) {
                return Females[user.Age - AgeMinimum].GetFirstUserWithCondition(ueueUser =>
                (ueueUser.preferences.ConnectionId == ConnID)) is object;
            }
            return Males[user.Age - AgeMinimum].GetFirstUserWithCondition(ueueUser =>
                (ueueUser.preferences.ConnectionId == ConnID)) is object;
        }


        internal ConcurrentLinkedListQueue<InQueueStatus>[] Males;
        internal ConcurrentLinkedListQueue<InQueueStatus>[] Females;
    }

}
