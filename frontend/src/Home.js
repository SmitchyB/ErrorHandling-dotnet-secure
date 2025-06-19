// Home.js (for Insecure Error Handling Demo - Simple UI)
import React, { useState } from 'react';
import logo from './logo.svg'; // Assuming you have a logo.svg in your src folder
import './Home.css';

function Home() {
  const [inputValue, setInputValue] = useState('');
  const [errorResponse, setErrorResponse] = useState(null);
  const [loading, setLoading] = useState(false);

  const handleInputChange = (e) => {
    setInputValue(e.target.value);
  };

  const handleSubmitError = async (e) => {
    e.preventDefault(); // Prevent default form submission to handle via fetch
    setErrorResponse(null); // Clear previous errors
    setLoading(true); // Show loading state
    console.log(`Attempting to trigger an error on backend with input: "${inputValue}"`);

    try {
      // Send a POST request to the backend's error-triggering endpoint
      const response = await fetch('http://localhost:5000/api/error/trigger', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ simulatedInput: inputValue }), // Send some dummy data
      });

      // Read the raw text response body, which should contain the stack trace
      const textResponse = await response.text(); 
      
      // Store the response details to display
      setErrorResponse({
        status: response.status,
        statusText: response.statusText,
        body: textResponse,
      });

      if (response.ok) {
        // This path is generally not expected for this demo's purpose
        console.log('Backend returned OK (unexpected for error demo):', textResponse);
      } else {
        // This path is expected for error demonstration
        console.error('Backend returned an error (expected):', textResponse);
      }
    } catch (error) {
      // Catch network errors or other issues preventing connection to backend
      console.error('Network or frontend error:', error);
      setErrorResponse({
        status: 'Network Error',
        body: error.message,
      });
      // Use a message box instead of alert() as per instructions
      // alert('Could not connect to the backend. Is it running?');
    } finally {
      setLoading(false); // End loading state
    }
  };

  return (
    <div className="home-container">
      <img src={logo} className="Home-logo" alt="logo" />
      
      <p className="Home-title">
        **Secure Error Handling Demonstration (.NET 8 Backend)**
      </p>

      <h2>Trigger a Server Error</h2>
      <form onSubmit={handleSubmitError} className="error-form">
        <div className="form-group">
          <label htmlFor="inputField">Enter any text (it won't prevent the error):</label>
          <input
            type="text"
            id="inputField"
            name="inputField"
            value={inputValue}
            onChange={handleInputChange}
            placeholder="e.g., product ID or a message"
            className="form-input"
            disabled={loading}
          />
        </div>
        <button type="submit" disabled={loading} className="submit-button">
          {loading ? 'Triggering Error...' : 'Submit to Trigger Error'}
        </button>
      </form>

      {errorResponse && (
        <div className="error-display">
          <h3>Server Response:</h3>
          <p><strong>HTTP Status:</strong> {errorResponse.status} {errorResponse.statusText}</p>
          <h4>Raw Response Body (Contains Insecure Details):</h4>
          <pre className="response-body-pre">{errorResponse.body}</pre>
        </div>
      )}
    </div>
  );
}

export default Home;