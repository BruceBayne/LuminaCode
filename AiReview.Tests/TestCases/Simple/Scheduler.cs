
/// <summary>
//	MaxRating: 10
/// </summary>

public sealed class MessageScheduler : IDisposable
{
	internal sealed class TimerCallbackMessage : ImmutableMessage
	{
		/// <summary>
		/// The scheduled date and time associated with the timer event.
		/// </summary>
		/// <remarks>
		/// Indicates when the timer event occurred, allowing queued messages for that date and time to be processed.
		/// </remarks>
		public readonly DateTime DateTime;

		/// <summary>
		///   Constructs the ScheduledMessage message.
		/// </summary>
		public TimerCallbackMessage(Channel publishChannel, DateTime dateTime) : base(publishChannel)
		{
			this.DateTime = dateTime;
		}
	}

	private readonly IMessageBusConnection messageBusConnection;
	private readonly SequentialThreadPoolDispatcher messageDispatcher = new SequentialThreadPoolDispatcher(nameof(MessageScheduler));

	private readonly SortedDictionary<DateTime, List<ImmutableMessage>> messages =
		new SortedDictionary<DateTime, List<ImmutableMessage>>();

	private Timer timer;
	private DateTime? nextExecutionTime;

	/// <summary>
	///   Constructs this component and connects to the specified <c>IMessageBus</c>.
	/// </summary>
	/// <param name="messageBus">The message bus to connect to.</param>
	public MessageScheduler(IMessageBus messageBus)
	{
		this.messageBusConnection = messageBus
									.Connect(nameof(MessageScheduler), this.messageDispatcher)
									.SubscribeToTopic<ScheduledMessage>(nameof(MessageScheduler), HandleScheduledMessage)
									.SubscribeToContentOfType<TimerCallbackMessage>(HandleTimerCallbackMessage);
	}

	/// <inheritdoc />
	public void Dispose()
	{
		this.timer?.Dispose();
		this.messageDispatcher?.Dispose();
		this.messageBusConnection.Dispose();
	}

	//TBD: Slower than using LOCK without Timer Event.
	private void HandleTimerCallbackMessage(TimerCallbackMessage msg, CancellationToken token)
		=> HandleFinalMessages(msg.DateTime);


	private void HandleScheduledMessage(ScheduledMessage scheduledMessage, CancellationToken token)
	{
		if (scheduledMessage.Payload != null)
		{
			DateTime deliveryDate = scheduledMessage.DeliveryDate;

			if (!this.messages.ContainsKey(deliveryDate))
			{
				this.messages[deliveryDate] = new List<ImmutableMessage>();
			}

			this.messages[deliveryDate].Add(scheduledMessage.Payload);

			AdjustTimer();
		}
	}

	private void AdjustTimer()
	{
		if (this.messages.Count == 0)
		{
			this.timer?.Dispose();
			return;
		}

		// Get the next delivery time
		DateTime nextTime = this.messages.Keys.First();

		if (this.nextExecutionTime != null && this.nextExecutionTime.Value <= nextTime)
		{
			return; // Timer is already set for the earliest delivery time
		}

		this.nextExecutionTime = nextTime;

		// Calculate the time to wait until the next message
		DateTime now = DateTime.UtcNow;
		TimeSpan dueTime = nextTime - now;

		if (dueTime <= TimeSpan.Zero)
		{
			// If the due time has already passed, handle the message immediately
			HandleFinalMessages(nextTime);
		}
		else
		{
			// Otherwise, set a timer to trigger at the right time
			this.timer?.Dispose();
			this.timer = new Timer(OnSystemTimer, state: nextTime, dueTime: dueTime, period: Timeout.InfiniteTimeSpan);
		}
	}

	private void OnSystemTimer(object state)
	{
		var dateTime = (DateTime)state;
		var timerCallbackMessage = new TimerCallbackMessage(Channel.CreateContentBased<TimerCallbackMessage>(), dateTime);
		this.messageBusConnection.Publish(timerCallbackMessage);
	}

	private void HandleFinalMessages(DateTime deliveryTime)
	{
		this.nextExecutionTime = null;

		if (!this.messages.TryGetValue(deliveryTime, out List<ImmutableMessage> messageList))
			return;

		// Process all messages for this delivery time
		foreach (ImmutableMessage message in messageList)
		{
			this.messageBusConnection.Publish(message);
		}

		// Remove the processed messages
		this.messages.Remove(deliveryTime);

		// Adjust the timer for the next delivery time
		AdjustTimer();
	}
}