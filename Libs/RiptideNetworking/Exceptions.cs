// This file is provided under The MIT License as part of RiptideNetworking.
// Copyright (c) Tom Weiland
// For additional information please see the included LICENSE.md file or view it on GitHub:
// https://github.com/RiptideNetworking/Riptide/blob/main/LICENSE.md

using Riptide.Utils;

namespace Riptide
{
    /// <summary>The exception that is thrown when a <see cref="Message"/> does not contain enough unwritten bits to perform an operation.</summary>
    public class InsufficientCapacityException : Exception
    {
        /// <summary>The message with insufficient remaining capacity.</summary>
        public readonly Message? RiptideMessage;
        /// <summary>The name of the type which could not be added to the message.</summary>
        public readonly string? TypeName;
        /// <summary>The number of available bits the type requires in order to be added successfully.</summary>
        public readonly int RequiredBits;

        /// <summary>Initializes a new <see cref="InsufficientCapacityException"/> instance.</summary>
        public InsufficientCapacityException() { }
        /// <summary>Initializes a new <see cref="InsufficientCapacityException"/> instance with a specified error message.</summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public InsufficientCapacityException(string message) : base(message) { }
        /// <summary>Initializes a new <see cref="InsufficientCapacityException"/> instance with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception. If <paramref name="inner"/> is not a null reference, the current exception is raised in a catch block that handles the inner exception.</param>
        public InsufficientCapacityException(string message, Exception inner) : base(message, inner) { }
        /// <summary>Initializes a new <see cref="InsufficientCapacityException"/> instance and constructs an error message from the given information.</summary>
        /// <param name="message">The message with insufficient remaining capacity.</param>
        /// <param name="reserveBits">The number of bits which were attempted to be reserved.</param>
        public InsufficientCapacityException(Message message, int reserveBits) : base(GetErrorMessage(message, reserveBits))
        {
            RiptideMessage = message;
            TypeName = "reservation";
            RequiredBits = reserveBits;
        }
        /// <summary>Initializes a new <see cref="InsufficientCapacityException"/> instance and constructs an error message from the given information.</summary>
        /// <param name="message">The message with insufficient remaining capacity.</param>
        /// <param name="typeName">The name of the type which could not be added to the message.</param>
        /// <param name="requiredBits">The number of available bits required for the type to be added successfully.</param>
        public InsufficientCapacityException(Message message, string typeName, int requiredBits) : base(GetErrorMessage(message, typeName, requiredBits))
        {
            RiptideMessage = message;
            TypeName = typeName;
            RequiredBits = requiredBits;
        }
        /// <summary>Initializes a new <see cref="InsufficientCapacityException"/> instance and constructs an error message from the given information.</summary>
        /// <param name="message">The message with insufficient remaining capacity.</param>
        /// <param name="arrayLength">The length of the array which could not be added to the message.</param>
        /// <param name="typeName">The name of the array's type.</param>
        /// <param name="requiredBits">The number of available bits required for a single element of the array to be added successfully.</param>
        public InsufficientCapacityException(Message message, int arrayLength, string typeName, int requiredBits) : base(GetErrorMessage(message, arrayLength, typeName, requiredBits))
        {
            RiptideMessage = message;
            TypeName = $"{typeName}[]";
            RequiredBits = requiredBits * arrayLength;
        }

        /// <summary>Constructs the error message from the given information.</summary>
        /// <returns>The error message.</returns>
        private static string GetErrorMessage(Message message, int reserveBits)
        {
            return $"Cannot reserve {reserveBits} {Helper.CorrectForm(reserveBits, "bit")} in a message with {message.UnwrittenBits} " +
                   $"{Helper.CorrectForm(message.UnwrittenBits, "bit")} of remaining capacity!";
        }
        /// <summary>Constructs the error message from the given information.</summary>
        /// <returns>The error message.</returns>
        private static string GetErrorMessage(Message message, string typeName, int requiredBits)
        {
            return $"Cannot add a value of type '{typeName}' (requires {requiredBits} {Helper.CorrectForm(requiredBits, "bit")}) to " +
                   $"a message with {message.UnwrittenBits} {Helper.CorrectForm(message.UnwrittenBits, "bit")} of remaining capacity!";
        }
        /// <summary>Constructs the error message from the given information.</summary>
        /// <returns>The error message.</returns>
        private static string GetErrorMessage(Message message, int arrayLength, string typeName, int requiredBits)
        {
            requiredBits *= arrayLength;
            return $"Cannot add an array of type '{typeName}[]' with {arrayLength} {Helper.CorrectForm(arrayLength, "element")} (requires {requiredBits} {Helper.CorrectForm(requiredBits, "bit")}) " +
                   $"to a message with {message.UnwrittenBits} {Helper.CorrectForm(message.UnwrittenBits, "bit")} of remaining capacity!";
        }
    }
}
