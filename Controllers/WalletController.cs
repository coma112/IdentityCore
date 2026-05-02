using IdentityCore.DTOs;
using IdentityCore.Entities;
using IdentityCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Transactions;

namespace IdentityCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly UserManager<Player> _userManager;

        public WalletController(IWalletService walletService, UserManager<Player> userManager)
        {
            _walletService = walletService;
            _userManager = userManager;
        }

        /// <summary>
        /// Gets the current player's wallet balance.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBalance()
        {
            string playerId = _userManager.GetUserId(User)!;
            Wallet wallet = await _walletService.GetWalletAsync(playerId);

            return Ok(new WalletResponse(wallet));
        }

        /// <summary>
        /// Deposits funds into the wallet. Minimum deposit amount is 10!
        /// </summary>
        [HttpPost("deposit")]
        [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
        {
            string playerId = _userManager.GetUserId(User)!;
            var transaction = await _walletService.DepositAsync(playerId, request.Amount);

            return Ok(new TransactionResponse(transaction));
        }

        /// <summary>
        /// Withdraws funds from the wallet.
        /// </summary>
        [HttpPost("withdraw")]
        [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Withdraw([FromBody] DepositRequest request)
        {
            string playerId = _userManager.GetUserId(User)!;
            var transaction = await _walletService.WithdrawAsync(playerId, request.Amount);

            return Ok(new TransactionResponse(transaction));
        }

        /// <summary>
        /// Gets the player's transaction history w/ pagination and optional filters.
        /// </summary>
        [HttpGet("transactions")]
        [ProducesResponseType(typeof(PagedResponse<TransactionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTransactions([FromQuery] TransactionHistoryRequest request)
        {
            var playerId = _userManager.GetUserId(User)!;
            var result = await _walletService.GetTransactionHistoryAsync(playerId, request);

            return Ok(result);
        }
    }
}
