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
            undefined = new ConcurrentLinkedListQueue<InQueueStatus>();
        }

        internal void Push(InQueueStatus user)
        {
            if(user.user.Age == -1)
            {
                undefined.Enqueue(user);
                return;
            }
            if(user.user.IsFemale)
            {
                Females[user.user.Age - AgeMinimum].Enqueue(user);
                return;
            }
            Males[user.user.Age - AgeMinimum].Enqueue(user);
        }

        internal InQueueStatus GetId(InQueueStatus qStatus)
        {
            if(qStatus.user.Age == -1)
            {
                Console.WriteLine("tutaj przyszedl");
                return undefined.GetFirstUserWithCondition(v => true); //TODO 
            }

            var requirements = qStatus.preferences;
            int avr = ((requirements.MinAge + requirements.MaxAge) / 2) - AgeMinimum;
            var user = qStatus.user;
            IEnumerable<string> NotAcceptable = 
                qStatus.recent
            .Select(item => item.TryGetTarget(out var status) ? status.preferences.ConnectionId : null).Concat(
                qStatus.disliked
            .Select(item => item.TryGetTarget(out var status) ? status.preferences.ConnectionId : null)
                ).Where(item => item != null)!;

            string ConnID = qStatus.preferences.ConnectionId;
            for (int i = requirements.MaxAge - AgeMinimum; i > requirements.MinAge - AgeMinimum; )
            {
                --i;
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

        private bool MatchCheck(InQueueStatus status, QueueUser user, IEnumerable<string> NotAcceptable, string ConnId)
        {
            return status.preferences.MinAge <= user.Age &&
            status.preferences.MaxAge >= user.Age &&
            (user.IsFemale ? status.preferences.AcceptFemale : status.preferences.AcceptMale) &&
            status.preferences.ConnectionId != ConnId &&

           !NotAcceptable.Contains(status.preferences.ConnectionId) &&
           (status.recent.SearchFromMostRecent(recent => MatchesId(recent, ConnId)) is null) &&
           (status.disliked.SearchFromMostRecent(disliked => MatchesId(disliked, ConnId)) is null);
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
            if(user.Age == -1)
            {
                return undefined.GetFirstUserWithCondition(ueueUser =>
               (ueueUser.preferences.ConnectionId == ConnID)) is object;
            }
            if (user.IsFemale) {
                return Females[user.Age - AgeMinimum].GetFirstUserWithCondition(ueueUser =>
                (ueueUser.preferences.ConnectionId == ConnID)) is object;
            }
            return Males[user.Age - AgeMinimum].GetFirstUserWithCondition(ueueUser =>
                (ueueUser.preferences.ConnectionId == ConnID)) is object;
        }


        internal ConcurrentLinkedListQueue<InQueueStatus>[] Males;
        internal ConcurrentLinkedListQueue<InQueueStatus>[] Females;
        internal ConcurrentLinkedListQueue<InQueueStatus> undefined;
    }

}
