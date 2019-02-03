using System;

namespace Core.ApprovalAbstraction
{
    public struct RequestState
    {
        public enum ResultType
        {
            Created = 0,
            InProgress = 1,
            Approved = 7,
            Rejected = 8
        }

        public static RequestState Created => new RequestState {Result = ResultType.Created};
        public static RequestState Approved => new RequestState {Result = ResultType.Approved};
        public static RequestState Rejected => new RequestState {Result = ResultType.Rejected};

        public ResultType Result { get; set; }
        public string SubState { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(SubState) ? Result.ToString() : $"{Result} and {SubState}";
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(RequestState other)
        {
            return Result == other.Result && string.Equals(SubState, other.SubState, StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Result * 397) ^ (SubState != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(SubState) : 0);
            }
        }

        public static bool operator ==(RequestState left, RequestState right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RequestState left, RequestState right)
        {
            return !left.Equals(right);
        }
    }
}