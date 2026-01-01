const PlayFab = require("playfab-sdk");

// Initialize PlayFab
PlayFab.settings.titleId = process.env.PLAYFAB_TITLE_ID;
PlayFab.settings.developerSecretKey = process.env.PLAYFAB_SECRET_KEY;

module.exports = async function (context, req) {
    context.log('Account deletion request received');

    // Only allow POST requests
    if (req.method !== 'POST') {
        context.res = {
            status: 405,
            body: { message: 'Method not allowed' }
        };
        return;
    }

    const { email, reason, timestamp } = req.body;

    // Validate email
    if (!email || !isValidEmail(email)) {
        context.res = {
            status: 400,
            body: { message: 'Valid email address is required' }
        };
        return;
    }

    try {
        // 1. Find the user by email in PlayFab
        const accountInfo = await getAccountByEmail(email);

        if (!accountInfo) {
            // Even if account not found, return success to prevent email enumeration
            context.log(`No account found for email: ${email}`);
            context.res = {
                status: 200,
                body: {
                    message: 'If an account exists with this email, a deletion request has been submitted.',
                    requestId: generateRequestId()
                }
            };
            return;
        }

        const playFabId = accountInfo.PlayFabId;
        context.log(`Found account for deletion: ${playFabId}`);

        // 2. Log the deletion request (for audit purposes)
        await logDeletionRequest(context, {
            playFabId,
            email,
            reason: reason || 'No reason provided',
            timestamp,
            requestId: generateRequestId()
        });

        // 3. Delete the user's data from PlayFab
        await deleteUserData(playFabId);

        // 4. Delete the PlayFab account
        await deletePlayFabAccount(playFabId);

        context.log(`Successfully deleted account: ${playFabId}`);

        context.res = {
            status: 200,
            body: {
                message: 'Account deletion request processed successfully.',
                requestId: generateRequestId()
            }
        };
    } catch (error) {
        context.log.error('Error processing deletion request:', error);

        context.res = {
            status: 500,
            body: {
                message: 'Failed to process deletion request. Please contact support@haircuthistory.com'
            }
        };
    }
};

function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

function generateRequestId() {
    return 'DEL-' + Date.now().toString(36).toUpperCase() + '-' + Math.random().toString(36).substr(2, 5).toUpperCase();
}

async function getAccountByEmail(email) {
    return new Promise((resolve, reject) => {
        PlayFab.PlayFabServer.GetUserAccountInfo({
            Email: email
        }, (error, result) => {
            if (error) {
                // Account not found is not an error for our purposes
                if (error.errorCode === 1001) { // AccountNotFound
                    resolve(null);
                } else {
                    reject(error);
                }
            } else {
                resolve(result.data.UserInfo);
            }
        });
    });
}

async function deleteUserData(playFabId) {
    return new Promise((resolve, reject) => {
        // Get all user data keys first
        PlayFab.PlayFabServer.GetUserData({
            PlayFabId: playFabId
        }, (error, result) => {
            if (error) {
                reject(error);
                return;
            }

            const keys = Object.keys(result.data.Data || {});

            if (keys.length === 0) {
                resolve();
                return;
            }

            // Delete all user data
            PlayFab.PlayFabServer.UpdateUserData({
                PlayFabId: playFabId,
                KeysToRemove: keys
            }, (error, result) => {
                if (error) {
                    reject(error);
                } else {
                    resolve(result);
                }
            });
        });
    });
}

async function deletePlayFabAccount(playFabId) {
    return new Promise((resolve, reject) => {
        PlayFab.PlayFabServer.DeletePlayer({
            PlayFabId: playFabId
        }, (error, result) => {
            if (error) {
                reject(error);
            } else {
                resolve(result);
            }
        });
    });
}

async function logDeletionRequest(context, data) {
    // Log to Application Insights or your preferred logging service
    context.log('ACCOUNT_DELETION_REQUEST', JSON.stringify(data));

    // You could also send to a storage table, send an email notification, etc.
    // For compliance, keep these logs for your required retention period
}
