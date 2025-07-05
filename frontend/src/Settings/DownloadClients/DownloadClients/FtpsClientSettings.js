import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { inputTypes } from 'Helpers/Props';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import translate from 'Utilities/String/translate';

const securityModeOptions = [
  { key: 0, value: 'Explicit (AUTH TLS)' },
  { key: 1, value: 'Implicit (SSL)' },
  { key: 2, value: 'None (Plain FTP)' }
];

const connectionModeOptions = [
  { key: 0, value: 'Passive' },
  { key: 1, value: 'Active' }
];

class FtpsClientSettings extends Component {
  render() {
    const {
      settings,
      onInputChange
    } = this.props;

    const {
      host,
      port,
      username,
      password,
      securityMode,
      connectionMode,
      validateCertificate,
      basePath,
      movieDirectory,
      priority,
      scanInterval
    } = settings;

    return (
      <div>
        <FormGroup>
          <FormLabel>Host</FormLabel>
          <FormInputGroup
            type={inputTypes.TEXT}
            name="host"
            value={host}
            onChange={onInputChange}
            helpText="FTPS server hostname or IP address"
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>Port</FormLabel>
          <FormInputGroup
            type={inputTypes.NUMBER}
            name="port"
            value={port}
            onChange={onInputChange}
            helpText="FTPS server port (default: 21)"
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>Username</FormLabel>
          <FormInputGroup
            type={inputTypes.TEXT}
            name="username"
            value={username}
            onChange={onInputChange}
            helpText="FTPS username"
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>Password</FormLabel>
          <FormInputGroup
            type={inputTypes.PASSWORD}
            name="password"
            value={password}
            onChange={onInputChange}
            helpText="FTPS password"
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>Security Mode</FormLabel>
          <FormInputGroup
            type={inputTypes.SELECT}
            name="securityMode"
            value={securityMode}
            values={securityModeOptions}
            onChange={onInputChange}
            helpText="FTPS security mode"
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>Connection Mode</FormLabel>
          <FormInputGroup
            type={inputTypes.SELECT}
            name="connectionMode"
            value={connectionMode}
            values={connectionModeOptions}
            onChange={onInputChange}
            helpText="FTP connection mode"
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>Validate Certificate</FormLabel>
          <FormInputGroup
            type={inputTypes.CHECK}
            name="validateCertificate"
            value={validateCertificate}
            onChange={onInputChange}
            helpText="Validate SSL/TLS certificate"
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>Base Path</FormLabel>
          <FormInputGroup
            type={inputTypes.TEXT}
            name="basePath"
            value={basePath}
            onChange={onInputChange}
            helpText="Base directory path on the FTPS server"
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>Movie Directory</FormLabel>
          <FormInputGroup
            type={inputTypes.TEXT}
            name="movieDirectory"
            value={movieDirectory}
            onChange={onInputChange}
            helpText="Directory containing movies"
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>Priority</FormLabel>
          <FormInputGroup
            type={inputTypes.NUMBER}
            name="priority"
            value={priority}
            onChange={onInputChange}
            helpText="Server priority (1-100)"
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>Scan Interval</FormLabel>
          <FormInputGroup
            type={inputTypes.NUMBER}
            name="scanInterval"
            value={scanInterval}
            onChange={onInputChange}
            helpText="Scan interval in minutes"
          />
        </FormGroup>
      </div>
    );
  }
}

FtpsClientSettings.propTypes = {
  settings: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default FtpsClientSettings;